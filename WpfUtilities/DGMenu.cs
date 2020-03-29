using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Reflection;
using System.Windows.Media.Imaging;
using WpfUtilities.Collections;

namespace WpfUtilities
{
    public class DGMenu<V>
    {
        List<DGColumnSaver> columnInfos = new List<DGColumnSaver>();
        DataGrid DG;
        
        
        public void SetGrid(DataGrid dg)
        {
            this.DG = dg;
            ContextMenu cm = new ContextMenu();

            string imgFolder = AppDomain.CurrentDomain.BaseDirectory + @"images\";

            try
            {
                string SaveImage = imgFolder + "save01.png";
                MenuItem mi1 = new MenuItem();
                mi1.Header = "Save Column Order";
                mi1.Icon = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(SaveImage, UriKind.Absolute))
                };
                mi1.Click += new System.Windows.RoutedEventHandler(mi1_Click);
                cm.Items.Add(mi1);

                string LoadImage = imgFolder + "load01.png";
                MenuItem mi2 = new MenuItem();
                mi2.Header = "Load Column Order";
                mi2.Icon = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(LoadImage, UriKind.Absolute))
                };
                mi2.Click += new System.Windows.RoutedEventHandler(mi2_Click);
                cm.Items.Add(mi2);

                string ExcelImage = imgFolder + "excel01.png";
                MenuItem mi3 = new MenuItem();
                mi3.Header = "Export to Excel";
                mi3.Icon = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(ExcelImage, UriKind.Absolute))
                };

                mi3.Click += new System.Windows.RoutedEventHandler(mi3_Click);
                cm.Items.Add(mi3);
                DG.ContextMenu = cm;
            }
            catch (Exception e)
            {
                Utils.LogError(e.Message);                
            }

        }

        
        void mi1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            columnInfos.Clear();
            DG.Columns.ToList().ForEach(x => columnInfos.Add(new DGColumnSaver(x)));            
            
            Utils.SaveSetting<List<DGColumnSaver>>("pledger", DG.Name + ".xml", columnInfos);
        }

        void mi2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                columnInfos = Utils.LoadSetting<List<DGColumnSaver>>("pledger", DG.Name + ".xml");
                foreach (DataGridColumn dgc in DG.Columns)
                {
                    int i = columnInfos.FirstOrDefault(x => x.Header == dgc.Header).DisplayIndex;
                    dgc.DisplayIndex = i;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("No saved settings for this grid \n" + ex.Message);
            }

        }

        void mi3_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            ExportToExcel<V, List<V>> ss = new ExportToExcel<V, List<V>>();

            

            ICollectionView view = CollectionViewSource.GetDefaultView(DG.ItemsSource);


            if (DG.ItemsSource is AsyncObservableCollection<V>)
            {
                ss.dataToPrint = new List<V>();
                ss.dataToPrint.AddRange((AsyncObservableCollection<V>)view.SourceCollection);
            }

            else
            {
                ss.dataToPrint = (List<V>)view.SourceCollection; 
            }
            
            ss.GenerateReport();

        }

        
    }

    public class DGColumnSaver
    {
        public object Header { get; set; }
        public int DisplayIndex { get; set; }

        public DGColumnSaver()
        {

        }
        public DGColumnSaver(DataGridColumn dgc)
        {
            Header = dgc.Header;
            DisplayIndex = dgc.DisplayIndex;
        }
    }
}
