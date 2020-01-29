using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfUtilities
{
    public class Employee : INotifyPropertyChanged
    {
        private string name;
        int age;

        public event PropertyChangedEventHandler PropertyChanged;
                
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                this.OnPropertyChanged();
            }
        }
        
        public int Age
        {
            get { return age; }
            set 
            { 
                age = value;
                this.OnPropertyChanged();
            }
        }

        public Employee(string n, int a)
        {
            Name = n;
            Age = a;
        }

        public static ObservableCollection<Employee> Load()
        {
            return new ObservableCollection<Employee> { new Employee("alan", 1), new Employee("Ben", 2), new Employee("Kyle", 3), new Employee("Stan", 4) };
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
