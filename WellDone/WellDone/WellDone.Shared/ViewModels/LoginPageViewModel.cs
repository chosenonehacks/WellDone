using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Text;
using WellDone.Services;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using System.Linq;      
using Windows.Security.Credentials;
using WellDone.DataModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Networking.Connectivity;

namespace WellDone.ViewModels
{
    public class LoginPageViewModel : ViewModel, ILoginPageViewModel
    {
        private MobileServiceUser user;
        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private IMobileServiceClient _mobileServiceClient;
        private INavigationService _navigationService;
        public LoginPageViewModel(IDialogService dialogService, IEventAggregator eventAggregator, IMobileServiceClient mobileServiceClient, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            _mobileServiceClient = mobileServiceClient;
            _navigationService = navigationService;

            
        }

        DelegateCommand<string> _singinCommand;
        public DelegateCommand<string> SingInCommand
        {
            get
            {
                if (_singinCommand != null)
                    return _singinCommand;
                _singinCommand = new DelegateCommand<string>
                    (
                        async (args) => { await AuthenticateAsync(args); },
                        (args) => { return true; }
                    );
                this.PropertyChanged += (s, e) => _singinCommand.RaiseCanExecuteChanged();
                return _singinCommand;
            }
        }

        public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {

              _navigationService.ClearHistory(); // Clears login page navigation history.

              if(user !=null)
              _navigationService.Navigate(Experiences.Main, user);

              
        }

        public override void OnNavigatedFrom(Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatedFrom(viewModelState, suspending);
        }


        private bool IsConnectedToInternet()
        {
        /// Property that returns the connection profile [ ie, availability of Internet ]
        /// Interface type can be [ 1,6,9,23,24,37,71,131,144 ]
        /// 1 - > Some other type of network interface.
        /// 6 - > An Ethernet network interface.
        /// 9 - > A token ring network interface.
        /// 23 -> A PPP network interface.
        /// 24 -> A software loopback network interface.
        /// 37 -> An ATM network interface.
        /// 71 -> An IEEE 802.11 wireless network interface.
        /// 131 -> A tunnel type encapsulation network interface.
        /// 144 -> An IEEE 1394 (Firewire) high performance serial bus network interface.
        /// 243/244 ->3G/Mobile Detect
        /// </summary>
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile != null)
                {
                    var interfaceType = profile.NetworkAdapter.IanaInterfaceType;


                    return interfaceType == 71 || interfaceType == 243 || interfaceType == 244 || interfaceType == 6;;

                }
                return false;
            
        }

        private bool _IsLogingStarted;
        public bool IsLogingStarted
        {
            get { return _IsLogingStarted; }
            set { SetProperty(ref _IsLogingStarted, value); }
        }


        private async System.Threading.Tasks.Task AuthenticateAsync(string provider)
        {
            IsLogingStarted = true;

            //string message;  
            string title = "Login";
            string exitCommand = "Exit";  
            string msg;
            var exit = new UICommand(exitCommand, e => { App.Current.Exit(); }); 

            // Use the PasswordVault to securely store and access credentials.
            PasswordVault vault = new PasswordVault();
            PasswordCredential credential = null;

            

            while (credential == null)
            {
                if(!IsConnectedToInternet())
                {
                    msg = "You must be conected to internet.";
                    _dialogService.Show(msg, title,exit);
                    break;
                }

                try
                {
                    // Try to get an existing credential from the vault.
                    credential = vault.FindAllByResource(provider).FirstOrDefault();
                
                    //Testing prupose
                    //vault.Remove(credential);
                    //credential = null;
                }
                catch (Exception)
                {
                    // When there is no matching resource an error occurs, which we ignore.
                }

                if (credential != null)
                {
                    // Create a user from the stored credentials.
                    user = new MobileServiceUser(credential.UserName);
                    credential.RetrievePassword();
                    user.MobileServiceAuthenticationToken = credential.Password;

                    // Set the user from the stored credentials.
                    _mobileServiceClient.CurrentUser = user;
                
                    try
                    {
                        // Try to return an item now to determine if the cached credential has expired.
                        await _mobileServiceClient.GetTable<WellDoneTask>().Take(1).ToListAsync();
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {                        
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            // Remove the credential with the expired token.
                            vault.Remove(credential);
                            credential = null;
                            continue;
                        }
                        else if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            credential = null;

                            msg = "You might have connection problems. Check your internet connection.";
                            _dialogService.Show(msg, title);
                            _navigationService.Navigate(Experiences.Login);
                            IsLogingStarted = false;
                            break;                            
                        }
                    }
                }                
                else
                {                    
                    try
                    {                        
                        user = await _mobileServiceClient.LoginAsync(provider);
                                            
                        credential = new PasswordCredential(provider,
                        user.UserId, user.MobileServiceAuthenticationToken);
                        vault.Add(credential);

                    }
                    catch (InvalidOperationException)
                    {                                                
                        msg = "You must log in. Login Required";
                        _dialogService.Show(msg, title);
                        _navigationService.Navigate(Experiences.Login);
                        IsLogingStarted = false;
                    }   
                }
            }
            if(credential != null)
            await CheckIfUserHasAnyData();
            
        }

        private async Task CheckIfUserHasAnyData()
        {
            var wellDoneTable = _mobileServiceClient.GetTable<WellDoneTask>();
            var currentServiceUserId = _mobileServiceClient.CurrentUser.UserId;
            var taskList = await wellDoneTable.Where(t => t.UserId == currentServiceUserId).ToListAsync();
            bool isSedded = false;
            
            if(taskList.Any() == false) //No user data
            {
                //Seed with first time data
                isSedded = await SeedWithFirstTimeData(currentServiceUserId);
                if (isSedded)
                {
                    _navigationService.Navigate(Experiences.Main, user);
                }
            }
            _navigationService.Navigate(Experiences.Main, user);
        }

        private async Task<bool> SeedWithFirstTimeData(string currentServiceUser)
        {
            IMobileServiceTable<WellDoneTask> wellDoneTable = _mobileServiceClient.GetTable<WellDoneTask>();
            IMobileServiceTable<ContextDescription> contextDescriptionsTable = _mobileServiceClient.GetTable<ContextDescription>();

            //create ContextDescirptions
            var context1 = new ContextDescription { Id = Guid.NewGuid().ToString(), Name = "@Home", UserId = currentServiceUser};               
            var context2 = new ContextDescription { Id = Guid.NewGuid().ToString(), Name = "@Work", UserId = currentServiceUser};
            
            List<ContextDescription> contextes = new List<ContextDescription>();

            contextes.Add(context1);
            contextes.Add(context2);

            //create WellDoneTasks
            List<WellDoneTask> tasks = new List<WellDoneTask>
            {
                new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "First Project", IsComplete = false, IsProject = true, DueDate = new DateTime(2015,03,30),ProjectId = null, ContextId = context1.Id, UserId = currentServiceUser},                
                new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "Second task", IsComplete = false, IsProject = false, ProjectId = null, ContextId = context2.Id, UserId = currentServiceUser  },
            };

            var taskWithProject = new WellDoneTask { Id = Guid.NewGuid().ToString(), Topic = "First task", IsComplete = false, IsProject = false, ProjectId = tasks.FirstOrDefault().Id, ContextId = context1.Id, UserId = currentServiceUser };

            tasks.Add(taskWithProject);

            //Add data to tables
            foreach (var context in contextes)
            {
                await contextDescriptionsTable.InsertAsync(context);
            }

            foreach (var task in tasks)
            {
                await wellDoneTable.InsertAsync(task);
            }
            
            
            return true;
        }

    }
}
