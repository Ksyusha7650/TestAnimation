using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestAnimation {
    public partial class tvar : Window {


        public tvar() { InitializeComponent(); }
        private void OnLoaded(object sender, RoutedEventArgs e) 
        { 
            var storyboard = FindResource("MyStoryboard") as Storyboard; 
            storyboard.Begin(); 
        } 
    }
        
    
}
    
    