using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Web.WebView2.Core;
using System.Collections.ObjectModel;

namespace mapAdvanced
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point mouseDownPosition;
        private string googleMapsApiKey = "AIzaSyBgE1BuVmviOHPw7wv8GZSevRSdie8oi3Y";
        private const string API_URL = "http://103.234.126.43:3080/dtmeter/info/esd_essd_info/all";

        private ObservableCollection<string> locations;
        List<string> dt_list = new List<string>();
        List<LocationData> extracted_data = new List<LocationData>();
        List<LocationData> send_data = new List<LocationData>();

        public class LocationData
        {
            public string dt_name { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
            public string dt_id { get; set; }
            public string dt_capacity_kva { get; set; }
            public string total_customer_count { get; set; }
            public string feeder_id { get; set; }
            public string district_name { get; set; }
            public string esd_name { get; set; }
            public string feeder_name { get; set; }
        }

        public class District
        {
            public string district_name { get; set; }
            public string district_code { get; set; }

        }

        public class ESD
        {
            public string esd_name { get; set; }
            public string esd_code { get; set; }
        }

        public class Feeder
        {
            public string feeder_name { get; set; }
            public string feeder_id { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebViewAsync();
            LoadData();

            locations = new ObservableCollection<string>();
            SelectedLocationList.ItemsSource = locations;
        }

        private async void InitializeWebViewAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async();
                string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapView.html");
                string htmlContent = File.ReadAllText(htmlFilePath);

                var locations = await FetchValidLocationsAsync();
                string locationJson = JsonSerializer.Serialize(locations);

                htmlContent = htmlContent.Replace("{googleMapsApiKey}", googleMapsApiKey)
                                         .Replace("{ locationJson }", locationJson);

                webView.CoreWebView2.NavigateToString(htmlContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // fetching all the data of the MRI using the API returning a list of data and also loading only the valid data in range of Bhutan
        private async Task<List<LocationData>> FetchValidLocationsAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(API_URL);
                    var locations = JsonSerializer.Deserialize<List<LocationData>>(response);

                    if (locations == null) return new List<LocationData>();

                    var validLocations = locations.Where(location =>
                    {
                        bool isLatParsed = double.TryParse(location.latitude, out double lat);
                        bool isLngParsed = double.TryParse(location.longitude, out double lng);

                        return isLatParsed && isLngParsed && IsValidLocation(lat, lng);
                    }).ToList();

                    extracted_data = validLocations;
                    return validLocations;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching location data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<LocationData>();
            }
        }

        // check whether the location is a valid location
        public bool IsValidLocation(double lat, double lng)
        {
            return lat >= 26 && lat <= 28 && lng >= 88 && lng <= 93;
        }


        // this is to load the District name > ESD name > Feeder name in the tree view list
        private async void LoadData()
        {
            try
            {
                Dispatcher.Invoke(() => progressLogin.Visibility = Visibility.Visible);

                using (HttpClient client = new HttpClient())
                {
                    string districtsUrl = "http://103.234.126.43:3080/dtmeters/district";
                    string districtJson = await client.GetStringAsync(districtsUrl);

                    List<District> districts = JsonSerializer.Deserialize<List<District>>(districtJson);

                    Dispatcher.Invoke(() => NavigationTreeView.Items.Clear());

                    foreach (District district in districts)
                    {
                        var districtItem = new TreeViewItem
                        {
                            Header = district.district_name,
                            Tag = 1,
                            Foreground = Brushes.White,

                        };

                        // Fetch ESDs
                        string esdUrl = $"http://103.234.126.43:3080/dtmeter/district/{district.district_code}";
                        string esdJson = await client.GetStringAsync(esdUrl);

                        List<ESD> esds = JsonSerializer.Deserialize<List<ESD>>(esdJson);

                        foreach (ESD esd in esds)
                        {
                            var style = new Style(typeof(TreeViewItem));
                            style.Setters.Add(new Setter(TreeViewItem.BackgroundProperty, Brushes.Transparent));
                            style.Triggers.Add(new Trigger
                            {
                                Property = UIElement.IsMouseOverProperty,
                                Value = true,
                                Setters = { new Setter(TreeViewItem.BackgroundProperty, Brushes.Gray) }
                            });

                            var esdItem = new TreeViewItem
                            {
                                Header = esd.esd_name,
                                Tag = 2,
                                Foreground = Brushes.White,
                                Style = style
                            };

                            // Fetch Feeders
                            string feederUrl = $"http://103.234.126.43:3080/dtmeter/esd/{esd.esd_code}";
                            string feederJson = await client.GetStringAsync(feederUrl);

                            List<Feeder> feeders = JsonSerializer.Deserialize<List<Feeder>>(feederJson);

                            foreach (Feeder feeder in feeders)
                            {
                                var feederItem = new TreeViewItem
                                {
                                    Header = feeder.feeder_name,
                                    Tag = 3,
                                    Foreground = Brushes.White,
                                    Style = style
                                };

                                esdItem.Items.Add(feederItem);
                            }

                            districtItem.Items.Add(esdItem);
                        }

                        Dispatcher.Invoke(() => NavigationTreeView.Items.Add(districtItem));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Dispatcher.Invoke(() => progressLogin.Visibility = Visibility.Collapsed);
            }
        }

        // currently not in use
        //public void loaddataonclick_trial(string header, string tag, string locationpath)
        //{
        //    list<locationdata> requireddata = new list<locationdata>();

        //    if (tag == "1")
        //    {
        //        requireddata = extracted_data.where(location =>
        //        {
        //            bool islatparsed = double.tryparse(location.latitude, out double lat);
        //            bool islngparsed = double.tryparse(location.longitude, out double lng);
        //            bool isdistrictmatch = string.equals(location.district_name, header, stringcomparison.ordinalignorecase);
        //            return islatparsed && islngparsed && isvalidlocation(lat, lng) && isdistrictmatch;
        //        }).tolist();

        //    }
        //    else if (tag == "2")
        //    {
        //        requireddata = extracted_data.where(location =>
        //        {
        //            bool islatparsed = double.tryparse(location.latitude, out double lat);
        //            bool islngparsed = double.tryparse(location.longitude, out double lng);
        //            bool isdistrictmatch = string.equals(location.esd_name, header, stringcomparison.ordinalignorecase);
        //            return islatparsed && islngparsed && isvalidlocation(lat, lng) && isdistrictmatch;
        //        }).tolist();

        //    }
        //    else if (tag == "3")
        //    {
        //        requireddata = extracted_data.where(location =>
        //        {
        //            bool islatparsed = double.tryparse(location.latitude, out double lat);
        //            bool islngparsed = double.tryparse(location.longitude, out double lng);
        //            bool isdistrictmatch = string.equals(location.feeder_name, header, stringcomparison.ordinalignorecase);
        //            return islatparsed && islngparsed && isvalidlocation(lat, lng) && isdistrictmatch;
        //        }).tolist();

        //    }
        //    addmarker(requireddata);
        //}


        // this will locat

        public void LoadDataOnClick(string locationPath)  // correctly load the specific data based on the user click on tree list
        {
            string[] pathParts = locationPath.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);

            List<LocationData> requiredData = new List<LocationData>();

            requiredData = extracted_data.Where(location =>
            {
                //bool isLatParsed = double.TryParse(location.latitude, out double lat);
                //bool isLngParsed = double.TryParse(location.longitude, out double lng);
                //bool isValidLocation = isLatParsed && isLngParsed && IsValidLocation(lat, lng);

                switch (pathParts.Length)
                {
                    case 1: // District level
                        return string.Equals(location.district_name, pathParts[0], StringComparison.OrdinalIgnoreCase);

                    case 2: // ESD level
                        return string.Equals(location.district_name, pathParts[0], StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(location.esd_name, pathParts[1], StringComparison.OrdinalIgnoreCase);

                    case 3: // Feeder level
                        return string.Equals(location.district_name, pathParts[0], StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(location.esd_name, pathParts[1], StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(location.feeder_name, pathParts[2], StringComparison.OrdinalIgnoreCase);

                    default:
                        return false;
                }
            }).ToList();


            // add marker should be called only if there is no previously added data in the list of send data
            AddMarker(requiredData);
            addDataToList(requiredData);

            //Console.WriteLine($"Location Path: {locationPath}");
            //Console.WriteLine($"PathParts: {pathParts.Length}");
            //Console.WriteLine($"Filtered Data Count: {requiredData.Count}");
        }


        // removing the particular button
        public void deleteDataOnClick(string locationPath)
        {
            string[] pathParts = locationPath.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
            List<LocationData> requiredData = new List<LocationData>();

            requiredData = send_data.Where(location =>
            {
                switch (pathParts.Length)
                {
                    case 1: // District level
                        return string.Equals(location.district_name, pathParts[0], StringComparison.OrdinalIgnoreCase);

                    case 2: // ESD level
                        return string.Equals(location.district_name, pathParts[0], StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(location.esd_name, pathParts[1], StringComparison.OrdinalIgnoreCase);

                    case 3: // Feeder level
                        return string.Equals(location.district_name, pathParts[0], StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(location.esd_name, pathParts[1], StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(location.feeder_name, pathParts[2], StringComparison.OrdinalIgnoreCase);

                    default:
                        return false;
                }
            }).ToList();

            RemoveMarkerList(requiredData);
            RemoveMarker(requiredData);
        }



        // adding data to the list to check for previously logged data
        private void addDataToList(List<LocationData> newData)
        {
            foreach (var initialData in newData)
            {

                if (!send_data.Any(data => data.dt_id == initialData.dt_id))
                {
                    send_data.Add(initialData);
                }
            }

            foreach (var my_data in send_data)
            {
                Console.WriteLine(my_data.feeder_name);
            }

            //Console.WriteLine("Number of Data");
            //Console.WriteLine(send_data.Count);
        }


        //removingthe data list from the send_data list
        private void RemoveMarkerList(List<LocationData> dataToBeRemoved)
        {
            foreach (var initialData in dataToBeRemoved)
            {
                var itemToRemove = send_data.FirstOrDefault(data => data.dt_id == initialData.dt_id);
                if (itemToRemove != null)
                {
                    send_data.Remove(itemToRemove);
                    Console.WriteLine("Data successfully removed");
                }
            }
        }

        private async void AddMarker(List<LocationData> data)
        {
            try
            {
                string script = $"addData({JsonSerializer.Serialize(data)})";

                if (webView.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(script);
                }
                else
                {
                    MessageBox.Show("Not working web view");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }

        private async void RemoveMarker(List<LocationData> removeDatas)
        {
            Console.WriteLine("Remove marker function called");

            try
            {
                string script = $"removeData({JsonSerializer.Serialize(removeDatas)})";

                if (webView.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(script);
                }
                else
                {
                    MessageBox.Show("Web View Not Initialized");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }

        // when any of the item is clicked inthe tree list this one is activated 
        async private void NavigationTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem != null)
            {
                string fullPath = GetItemPath(selectedItem);
                string itemTag = selectedItem.Header?.ToString();

                if (!dt_list.Contains(itemTag))
                {
                    dt_list.Add(itemTag);
                }

                AddLocation(fullPath);
                AddToJavascript(String.Join(",", dt_list));
                LoadDataOnClick(fullPath);
            }
        }

        // determine the path of the clicked locaiton of the item in the tree list
        private string GetItemPath(TreeViewItem item)
        {
            List<string> pathParts = new List<string>();

            pathParts.Add(item.Header.ToString());

            TreeViewItem currentItem = item;

            while (currentItem.Parent is TreeViewItem parentItem)
            {
                pathParts.Insert(0, parentItem.Header.ToString());
                currentItem = parentItem;
            }

            return string.Join(">", pathParts);
        }

        // this is used to created that top bottom while updating the new 
        public void AddLocation(string location)
        {
            string[] pathParts = location.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);

            if (!locations.Contains(location))
            {
                //locations.Add(pathParts[pathParts.Length - 1]);
                locations.Add(location);
            }
        }

        // remove on click
        private void RemoveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string locationToRemove = button.CommandParameter as string;

            locations.Remove(locationToRemove); // this is to remove from the button physically from the list on the top nav
            
            Console.WriteLine("location to remove---->");
            Console.WriteLine(locationToRemove);

            dt_list.Remove(locationToRemove); // updated the list
            
            AddToJavascript(String.Join(", ", dt_list)); // some kind of javascript function
            deleteDataOnClick(locationToRemove);
        }


        // is this funciton used?
        private async void AddToJavascript(string dtListNames)
        {
            try
            {
                string script = $"addMarker({System.Text.Json.JsonSerializer.Serialize(dtListNames)})";

                if (webView.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(script);
                }
                else
                {
                    MessageBox.Show("Web View Not Initialized");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
            }
        }

        // Button handler
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Join(", ", dt_list));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dt_list.Clear();
        }

        private void cb1_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void cb1_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        // Access the javascript function using this
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
        }

        // Feeder and Mri butons view
        private void cb2_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cb2_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        // Window management code remains unchanged (previous implementation)
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                mouseDownPosition = e.GetPosition(this);
            }
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                if (Math.Abs(currentPosition.X - mouseDownPosition.X) > 5 ||
                    Math.Abs(currentPosition.Y - mouseDownPosition.Y) > 5)
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                        Point mousePosition = e.GetPosition(this);
                        Left = mousePosition.X - (ActualWidth / 2);
                        Top = mousePosition.Y - (40 / 2);
                    }
                    DragMove();
                }
            }
        }

        private void Header_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ToggleMaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        async private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var value = extracted_data.Count;
            foreach (var data in extracted_data)
            {
                Console.WriteLine(data.esd_name);
            }
        }

    }
}
