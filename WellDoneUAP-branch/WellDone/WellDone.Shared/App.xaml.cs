using Microsoft.Practices.Prism.Mvvm;
//using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WellDone.Messages;
using WellDone.Services;
using WellDone.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace WellDone
{
    public sealed partial class App : MvvmAppBase
    {

        static readonly UnityContainer Container = new UnityContainer();
        public App()
        {
            this.InitializeComponent();
            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs arg)
        {
            DebugThis(arg.Exception.ToString());

            var dialogService = Container.Resolve<IDialogService>();
            var resourceLoader = Container.Resolve<IResourceLoader>();

            // do not let the application exit unexpectedly
            arg.Handled = true;

            // terminal exception
                
                string content = resourceLoader.GetString("TerminalException-Content");
                string title = resourceLoader.GetString("TerminalException-Title");
                string exitCommand = resourceLoader.GetString("TerminalException-ExitCommand");
                var exit = new UICommand(exitCommand, e => { Current.Exit(); });
                dialogService.Show(content, title, exit);
        }

        protected override System.Threading.Tasks.Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Container.RegisterInstance(NavigationService);
            //Container.RegisterInstance<IEventAggregator>(new EventAggregator());
            Container.RegisterInstance<IResourceLoader>(new ResourceLoaderAdapter(new ResourceLoader()));

            Container.RegisterType<INavigationService, NavigationService>(new ContainerControlledLifetimeManager());            
            Container.RegisterType<IDialogService, DialogService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMainPageViewModel, MainPageViewModel>(new ContainerControlledLifetimeManager());            
            
            Container.RegisterType<IMobileServiceClient, MobileServiceClient>
                (new InjectionConstructor("https://welldone.azure-mobile.net/", "QbuqbitJWMuHnMcCrgvURhavAbLcqe33"));

            

            return Task.FromResult<object>(null);
        }

        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {

            var navigationService = Container.Resolve<INavigationService>();
            navigationService.Navigate(Experiences.Login);

            //Container.Resolve<IEventAggregator>().GetEvent<Logout>().Publish("You need to log in!");

            return Task.FromResult<object>(null);
        }
//Methods related with hardware back button login issue on windows phone
#if WINDOWS_PHONE_APP

        protected override void OnActivated(IActivatedEventArgs args)
        {
            var navigationService = Container.Resolve<INavigationService>();
            var mobileServiceClient = Container.Resolve<IMobileServiceClient>();
            


            ContinueWebAuthentication(args as WebAuthenticationBrokerContinuationEventArgs);

            //if (args.Kind == ActivationKind.WebAuthenticationBrokerContinuation)
            //{
            //        WebAuthenticationBrokerContinuationEventArgs argsi = args as WebAuthenticationBrokerContinuationEventArgs;
                    
            //        WebAuthenticationResult result = argsi.WebAuthenticationResult;

            //        if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
            //            NavigationService.GoBack();

            //        mobileServiceClient.LoginComplete(args as WebAuthenticationBrokerContinuationEventArgs);
            //}

            base.OnActivated(args);
        }


        public void ContinueWebAuthentication(WebAuthenticationBrokerContinuationEventArgs args)
    {
        var navigationService = Container.Resolve<INavigationService>();   
        var mobileServiceClient = Container.Resolve<IMobileServiceClient>();
        
        WebAuthenticationResult result = args.WebAuthenticationResult;
        string output; 
        
        // Process the authentication result	
        switch (result.ResponseStatus) 
        {
            case WebAuthenticationStatus.Success:           
                // Parse the token out of the authentication response
                output = result.ResponseData; 
                // Do something with the results token
                mobileServiceClient.LoginComplete(args);
                break;
            case WebAuthenticationStatus.UserCancel:
                // Handle user cancel                
                //navigationService.Navigate(Experiences.Login);
                App.Current.Exit();
                break;
            case WebAuthenticationStatus.ErrorHttp:
                // Http error
                DebugThis(WebAuthenticationStatus.ErrorHttp.ToString());
                //output = WebAuthenticationStatus.ErrorHttp.ToString(); 
                break;
            default:
                break;
        }
    }
#endif
        protected override object Resolve(Type type)
        {
            DebugThis(type.ToString());

            try
            {
                return Container.Resolve(type);
            }
            catch (Exception)
            {
                Debugger.Break();
                throw;
            }
        }

        void DebugThis(string text = null, [CallerMemberName] string caller = null)
        {
            Debug.WriteLine("{0} {1}", caller, text);
        }
    }
}