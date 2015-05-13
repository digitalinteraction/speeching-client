using SpeechingShared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Windows_Speeching.Common
{
    public static class WindowsUtils
    {
        public static async Task PrepareApp()
        {
            AppData.checkForConnection = () =>
            {
                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
                return internet;
            };

            AppData.onConnectionSuccess = () =>
            {

            };

            AppData.IO = new Win8PCLHelper();

            await AppData.AssignCacheLocations();
            await AppData.InitializeIfNeeded();

        }

        public static DependencyObject FindChildControl<T>(DependencyObject control, string ctrlName)
        {
            int childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childNumber; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(control, i);
                FrameworkElement fe = child as FrameworkElement;
                // Not a framework element or is null
                if (fe == null) return null;

                if (child is T && fe.Name == ctrlName)
                {
                    // Found the control so return
                    return child;
                }
                else
                {
                    // Not found it - search children
                    DependencyObject nextLevel = FindChildControl<T>(child, ctrlName);
                    if (nextLevel != null)
                        return nextLevel;
                }
            }
            return null;
        }
    }
}
