using NLog;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ZapReport.Components;
using ZapReport.Helpers;
using ZapSurgical.Data;
using ZapTranslation;

namespace ZapReport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private ZapClient.ZapClient _client;
        private PlanConfig _config;

        private List<Patient> _patientList;
        private List<Plan> _planList;
        private List<User> _userList;

        private Dictionary<string, Type> _listOfPrintComponents;
        private ObservableCollection<string> _includedComponents = new ObservableCollection<string>();
        private ObservableCollection<string> _excludedComponents = new ObservableCollection<string>();
        private string _filename = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += Init;
        }

        private void Init(object sender, RoutedEventArgs e)
        {
            try
            {
                CreateConfig();
            }
            catch (FileNotFoundException _)
            {
                while (MessageBox.Show(String.Format(Translate.GetString("FileNotFoundErrorText"), "ZapReport.cfg"), Translate.GetString("FileNotFoundErrorCaption"), MessageBoxButton.OK, MessageBoxImage.Error) != MessageBoxResult.OK)
                    ;

                Application.Current.Shutdown();
                return;
            }

            CreateLogger();

            _client = new ZapClient.ZapClient(GetUsernameAndPassword, _logger);

            if (!_client.OpenConnection())
            {
                Close();
                return;
            }

            // Check for parameters

            // Set default language
            if (!string.IsNullOrEmpty(_config?.Culture))
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(_config.Culture);
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(_config.Culture);
            }

            // Localization
            Title = Translate.GetString("ReportGenerator");
            lblPatient.Content = Translate.GetString("Patient");
            cbArchived.Content = Translate.GetString("Archived");
            lblPlan.Content = Translate.GetString("Plan");
            lblFraction.Content = Translate.GetString("Fraction");
            lblPhysician.Content = Translate.GetString("Physician");
            lblPhysicist.Content = Translate.GetString("Physicist");
            lblPrint.Content = Translate.GetString("Print");
            lblVersion.Content = Translate.GetString("Version") + $" {System.Reflection.Assembly.GetEntryAssembly().GetName().Version}";

            lbExcludedComponents.ItemsSource = _excludedComponents;
            lbIncludedComponents.ItemsSource = _includedComponents;

            _listOfPrintComponents = GetNameOfPrintComponents();

            foreach (var name in _listOfPrintComponents.Keys.OrderBy(s => s))
            {
                _excludedComponents.Add(name);
            }

            foreach (var item in _config.ReportTypes)
            {
                cbPrintType.Items.Add(item.Title);
            }

            if (cbPrintType.Items.Count > 0)
                cbPrintType.SelectedIndex = 0;

            btnRefresh.Content = Translate.GetString("Refresh");
            btnCancel.Content = Translate.GetString("Close");
            btnOk.Content = Translate.GetString("PrintPDF");

            UpdatePatients();
            UpdateUsers();

            dpPhysician.IsDropDownOpen = false;
            dpPhysician.SelectedDate = System.DateTime.Now;
            dpPhysicist.IsDropDownOpen = false;
            dpPhysicist.SelectedDate = System.DateTime.Now;

            SizeToContent = SizeToContent.WidthAndHeight;

            cbPatient.Focus();
        }

        private void UpdatePatients()
        {
            _logger.Info("Update patients");

            // Save selected values
            var selectedPatientItem = (string)cbPatient.SelectedItem;
            var selectedMedicalId = selectedPatientItem?.Substring(0, selectedPatientItem.IndexOf(" - "));
            var selectedPlanItem = (string)cbPlan.SelectedItem;
            
            Patient selectedPatient = null;

            cbPatient.SelectionChanged -= cbPatient_SelectionChanged;

            cbPatient.Items.Clear();
            cbPatient.SelectedIndex = 0;

            // Get all patients from Broker
            if ((bool)cbArchived.IsChecked)
                _patientList = _client.GetAllPatients();
            else
                _patientList = _client.GetPatientsWithStatus();

            foreach (var patient in _patientList?.OrderBy(p => p.MedicalId).Reverse())
            {
                var text = CheckString(patient.MedicalId) + " - " + CheckString(patient.LastName) + ", " + CheckString(patient.FirstName) + (CheckString(patient.MiddleName) != string.Empty ? " " + patient.MiddleName : "");
                cbPatient.Items.Add(text);

                if (patient.MedicalId == selectedMedicalId)
                    selectedPatient = patient;
            }

            cbPatient.SelectionChanged += cbPatient_SelectionChanged;

            // Select old selected patient
            if (cbPatient.Items.Contains(selectedPatientItem))
            {
                cbPatient.SelectedIndex = cbPatient.Items.IndexOf(selectedPatientItem);
                UpdatePlansForPatient(GetPatientForMedicalId(selectedMedicalId), selectedPlanItem);
            }
        }

        private void UpdateUsers()
        {
            _logger.Info("Update users");

            // Save selected users
            var selectedPhysician = (string)cbPhysician.SelectedItem;
            var selectedPhysicist = (string)cbPhysicist.SelectedItem;

            cbPhysician.Items.Clear();
            cbPhysician.SelectedIndex = 0;
            cbPhysicist.Items.Clear();
            cbPhysicist.SelectedIndex = 0;

            _userList = _client.GetTPSUsers();

            foreach (var user in _userList?.Where(u => u.Status == UserStatus.Valid))
            {
                foreach (var group in user.GroupNames)
                {
                    if (group == "MDS")
                        cbPhysician.Items.Add(user.Name);
                    if (group == "PHY")
                        cbPhysicist.Items.Add(user.Name);
                }
            }

            if (cbPhysician.Items.Contains(selectedPhysician))
                cbPhysician.SelectedItem = selectedPhysician;
            if (cbPhysicist.Items.Contains(selectedPhysicist))
                cbPhysicist.SelectedItem = selectedPhysicist;

            var physician = _userList.Where(u => u.Status == UserStatus.Valid && u.Name.Trim() == (string)cbPhysician.SelectedItem).FirstOrDefault();
            var physicist = _userList.Where(u => u.Status == UserStatus.Valid && u.Name.Trim() == (string)cbPhysicist.SelectedItem).FirstOrDefault();

            if (physician != null)
            {
                var filenameExists = File.Exists(System.IO.Path.Combine("signs", physician.UserID + ".png"));

                cbPhysicianSign.IsEnabled = filenameExists;
                cbPhysicianSign.IsChecked = filenameExists;
            }
            else
            {
                cbPhysicianSign.IsEnabled = false;
                cbPhysicianSign.IsChecked = false;
            }

            if (physicist != null)
            {
                var filenameExists = File.Exists(System.IO.Path.Combine("signs", physicist.UserID + ".png"));

                cbPhysicistSign.IsEnabled = filenameExists;
                cbPhysicistSign.IsChecked = filenameExists;
            }
            else
            {
                cbPhysicistSign.IsEnabled = false;
                cbPhysicistSign.IsChecked = false;
            }
        }

        private void UpdatePlansForPatient(Patient patient, string selectedPlan = "")
        {
            _logger.Info($"Update plans for patient '{patient.MedicalId.Trim()}'");

            cbPlan.Items.Clear();
            _planList = null;

            if (patient == null)
            {
                _logger.Error($"Patient '{patient.MedicalId.Trim()}' not found");
                return;
            }

            _planList = _client.GetPlansForPatient(patient);

            foreach (var plan in _planList)
            {
                if (string.IsNullOrEmpty(plan.PlanName))
                    continue;

                cbPlan.Items.Add(CheckString(plan.PlanName));
            }

            if (cbPlan.Items.Contains(selectedPlan))
            {
                cbPlan.SelectedItem = selectedPlan;
            }
            else if (cbPlan.Items.Count > 0)
            {
                cbPlan.SelectedIndex = 0;
            }
        }

        private void UpdateFractions(bool enabled)
        {
            lblFraction.IsEnabled = enabled;
            cbFraction.IsEnabled = enabled;

            cbFraction.Items.Clear();
        }

        public (string, string) GetUsernameAndPassword(string oldUsername, string oldPassword)
        {
            var dialog = new LoginWindow();

            dialog.lblUsername.Content = Translate.GetString("Username");
            dialog.lblPassword.Content = Translate.GetString("Password");
            dialog.btnLogin.Content = Translate.GetString("Login");
            dialog.btnCancel.Content = Translate.GetString("Cancel");

            dialog.textUsername.Text = oldUsername;
            dialog.textPassword.Password = oldPassword;

            if (string.IsNullOrEmpty(oldUsername))
            {
                dialog.textUsername.Focus();
            }
            else
            {
                dialog.textPassword.Focus();
            }

            if (!(bool)dialog.ShowDialog())
            {
                return (oldUsername, oldPassword);
            }

            return (dialog.textUsername.Text, dialog.textPassword.Password);
        }

        private Patient GetPatientForMedicalId(string medicalId)
        {
            Patient result = null;

            foreach (var patient in _patientList)
            {
                if (patient.MedicalId.Trim() == medicalId)
                    result = patient;
            }

            return result;
        }

        private void CreateConfig()
        {
            _config = PlanConfig.LoadConfigData();
        }

        private void CreateLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location), "log").Replace(".log", "." + ZapClient.Helpers.Network.GetHostName() + ".log").Replace("..", ".") };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        private void cbPatient_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedPatient = (string)cbPatient.SelectedItem;
            var selectedMedicalId = selectedPatient.Substring(0, selectedPatient.IndexOf(" - "));

            UpdatePlansForPatient(GetPatientForMedicalId(selectedMedicalId));
        }

        private void cbPlan_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var plan = _planList?.Where(p => p.PlanName == (string)cbPlan.SelectedItem).FirstOrDefault();

            if (plan is null)
            {
                UpdateFractions(false);
                return;
            }

            var planData = _client.GetPlanDataForPlan(plan);
            var user = _userList?.Where(u => u.UserID == planData.Approver).FirstOrDefault();

            if (user != null && cbPhysician.Items != null && cbPhysician.Items.Contains(user.Name))
            {
                cbPhysician.SelectedItem = user.Name;
            }

            if (plan.Status >= ZapSurgical.Data.PlanStatus.Deliverable && plan.Status < ZapSurgical.Data.PlanStatus.Deleted)
            {
                dpPhysician.SelectedDate = plan.ApprovalTime.Date;
            }
            else
            {
                dpPhysician.SelectedDate = plan.LastSavedTime.Date;
            }

            // Check if there is a treatment for this plan
            var deliveryData = _client.GetDeliveryDataForPlan(plan, true);

            if (deliveryData == null)
            {
                UpdateFractions(false);
                return;
            }

            UpdateFractions(true);

            cbFraction.Items.Add(Translate.GetString("All"));

            for (var i = 0; i < deliveryData.Fractions.Count; i++)
            {
                cbFraction.Items.Add((i+1).ToString() + (deliveryData.Fractions[i].IsMakeup ? "(" + Translate.GetString("Makeup Fraction") + ")" : ""));
            }

            cbFraction.SelectedIndex = 0;
        }

        private void cbPhysician_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var physician = _userList?.Where(u => u.Status == UserStatus.Valid && u.Name.Trim() == (string)cbPhysician.SelectedItem).FirstOrDefault();

            if (physician == null)
                return;

            var filenameExists = File.Exists(System.IO.Path.Combine("signs", physician.UserID + ".png"));

            cbPhysicianSign.IsEnabled = filenameExists;
            cbPhysicianSign.IsChecked = filenameExists;
        }

        private void cbPhysicist_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var physicist = _userList?.Where(u => u.Status == UserStatus.Valid && u.Name.Trim() == (string)cbPhysicist.SelectedItem).FirstOrDefault();

            if (physicist == null)
                return;

            var filenameExists = File.Exists(System.IO.Path.Combine("signs", physicist.UserID + ".png"));

            cbPhysicistSign.IsEnabled = filenameExists;
            cbPhysicistSign.IsChecked = filenameExists;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            var selectedPatient = (string)cbPatient.SelectedItem;
            var medicalId = selectedPatient.Substring(0, selectedPatient.IndexOf(" - "));

            var patient = _patientList?.Where(p => p.MedicalId.Trim() == medicalId).FirstOrDefault();

            if (cbPlan.SelectedItem == null)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("No plan selected", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }

            var plan = _planList?.Where(p => p.PlanName.Trim() == (string)cbPlan.SelectedItem).FirstOrDefault();
            var physician = _userList?.Where(u => u.Status == UserStatus.Valid && u.Name.Trim() == (string)cbPhysician.SelectedItem).FirstOrDefault();
            var physicist = _userList?.Where(u => u.Status == UserStatus.Valid && u.Name.Trim() == (string)cbPhysicist.SelectedItem).FirstOrDefault();

            _logger.Info($"Create report for patient '{medicalId}', plan name '{plan.PlanName}'");

            int fraction = cbFraction.IsEnabled ? (cbFraction.SelectedIndex == 0 ? 0 : cbFraction.SelectedIndex) : -1;

            var printData = new PrintData(_client, patient, plan)
            {
                DeliveredFraction = fraction,
                Physician = physician,
                Physicist = physicist,
                DatePhysician = (System.DateTime)dpPhysician.SelectedDate,
                DatePhysicist = (System.DateTime)dpPhysicist.SelectedDate
            };

            _client.CalcDoseForIsocenters(printData.PlanBeamData, printData.PlanSystemData);

            var caption = cbPrintType.SelectedItem?.ToString();
            var filename = _filename;

            filename = filename.Replace("{MedicalId}", patient.MedicalId.Trim());
            filename = filename.Replace("{PatientLastName}", patient.LastName.Trim());
            filename = filename.Replace("{PatientFirstName}", patient.FirstName.Trim());
            filename = filename.Replace("{PatientMiddleName}", patient.MiddleName.Trim());
            filename = filename.Replace("{PlanName}", plan.PlanName.Trim());
            filename = _config.Folder.Trim() + System.IO.Path.DirectorySeparatorChar + filename + ".pdf";

            var report = new PlanReport(_config, printData, _listOfPrintComponents, _includedComponents.ToList(), caption, cbPhysicianSign.IsChecked, cbPhysicistSign.IsChecked);
            try
            {
                report.GeneratePdf(filename);
            }
            catch (Exception except)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Error creating PDF file: {except.Message}\n\n{except.StackTrace}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                _logger.Error($"Error creating PDF file: {except.Message}");
                _logger.Error($"Error creating PDF file: Stack trace\n{except.StackTrace}");
                _logger.Error($"Error creating PDF file: Inner exception\n{except.InnerException.Message}");
            }
            finally
            {
                report = null;
                printData = null;

                Mouse.OverrideCursor = null;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _client?.CloseConnection();

            Close();
        }

        private void cbArchived_Click(object sender, RoutedEventArgs e)
        {
            UpdatePatients();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Do this, that we could be sure, that connection is
            // open even if the timeout closed the old one
            _client?.CloseConnection();
            _client?.OpenConnection();

            UpdatePatients();
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (lbIncludedComponents.SelectedItem == null || lbIncludedComponents.SelectedIndex == 0)
                return;

            var index = lbIncludedComponents.SelectedIndex;

            _includedComponents.Move(index, index - 1);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (lbIncludedComponents.SelectedItem == null || lbIncludedComponents.SelectedIndex == _includedComponents.Count - 1)
                return;

            var index = lbIncludedComponents.SelectedIndex;

            _includedComponents.Move(index, index + 1);
        }

        private void btnInclude_Click(object sender, RoutedEventArgs e)
        {
            if (lbExcludedComponents.SelectedItem == null)
                return;

            var selectedIndex = lbExcludedComponents.SelectedIndex;
            var selectedItem = (string)lbExcludedComponents.SelectedItem;

            _excludedComponents.RemoveAt(selectedIndex);
            _includedComponents.Add(selectedItem);

            lbIncludedComponents.SelectedIndex = _includedComponents.Count - 1;
        }

        private void btnExclude_Click(object sender, RoutedEventArgs e)
        {
            if (lbIncludedComponents.SelectedItem == null)
                return;

            var selectedIndex = lbIncludedComponents.SelectedIndex;
            var selectedItem = (string)lbIncludedComponents.SelectedItem;

            _includedComponents.RemoveAt(selectedIndex);
            if (!selectedItem.Equals("PageBreak"))
                _excludedComponents.Add(selectedItem);

            _excludedComponents = new ObservableCollection<string>(_excludedComponents.OrderBy(s => s));
            lbExcludedComponents.ItemsSource = _excludedComponents;
        }

        private void btnPageBreak_Click(object sender, RoutedEventArgs e)
        {
            _includedComponents.Add("PageBreak");

            lbIncludedComponents.SelectedIndex = _includedComponents.Count - 1;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var entry = _config.ReportTypes.Where(t => t.Title == (string)cbPrintType.SelectedItem).FirstOrDefault();

            if (entry == null)
                return;

            entry.Components.Clear();

            foreach (var item in _includedComponents)
            {
                entry.Components.Add(_listOfPrintComponents[item].Name);
            }

            PlanConfig.SaveConfigData(_config);
        }

        private void cbPrintType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var title = (string)cbPrintType.SelectedItem;
            var reportTypeData = _config.ReportTypes.Where(t => t.Title == title).FirstOrDefault();

            _filename = reportTypeData.Filename;

            var includedComponents = new List<string>(_includedComponents.Count);

            foreach (var item in _includedComponents)
            {
                includedComponents.Add((string)item);
            }

            includedComponents = _includedComponents.ToList();

            foreach (var item in includedComponents)
            {
                _includedComponents.Remove((string)item);
                _excludedComponents.Add((string)item);
            }

            foreach (var item in reportTypeData.Components) 
            {
                var text = Translate.GetString(item);
                _excludedComponents.Remove(text);
                _includedComponents.Add(text);
            }

            _excludedComponents = new ObservableCollection<string>(_excludedComponents.OrderBy(s => s));
            lbExcludedComponents.ItemsSource = _excludedComponents;
            lbExcludedComponents.SelectedItem = null;
        }

        private string CheckString(string text)
        {
            if (text == null)
                return string.Empty;

            return string.IsNullOrWhiteSpace(text.Trim()) ? string.Empty : text.Trim();
        }

        public Dictionary<string, Type> GetNameOfPrintComponents() 
        {
            var result = new Dictionary<string, Type>();

            foreach (var c in System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsClass && t.BaseType == typeof(PrintComponent) && t.Namespace == "ZapReport.Components").ToList())
            {
                var fieldInfo = c.GetField("ComponentCaption", BindingFlags.Public | BindingFlags.Static);
                var key = (string)fieldInfo.GetValue(c);

                if (fieldInfo == null || key == null)
                    continue;
                
                result.Add(key, c);
            }

            return result;
        }
    }
}
