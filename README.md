# ZapReport

## Introduction
ZapReport is a tool to print planning and treatment reports for the Zap-X system. It replaces the build-in report system. Treatment reports can be printed without access to the Zap-X.

## Installation
1.	You receive a ZIP file. Unzip the zip-file or copy its content into a folder on your hard disk.
2.	Copy your logo (PNG file, size around 300 px x 150 px) into the sub-folder “Logos“
3.	Copy your scanned signs (PNG files, good quality) into sub-folder “Signs“. Name it after the UserID of the user. Normally this are the initials of the user.
4.	Open the folder in Windows Explorer, search for the file “ZapClient.cfg“ and open it for editing
5.	Search for the entry “Server“ and replace the IP address with the IP address for your broker. Normally this is the DatabasePC (10.0.0.105 in the Zap network). If you want to reach the broker from outside of the Zap network, use the IP address of the firewall. If you want to use it from inside and outside, you have to make a CFG file for each PC you want to use. Best is to have one or two CFG files from inside the Zap network (should be already in the ZIP file) and use the normal “ZapClient.cfg“ for the outside connection.
6.	If you don’t want to be asked for  user and password each time you start the program: edit the fields „Username“ and „Password“ and provide the correct entries. If you setup more than one CFG file, you have to edit all of them to provide credentials.
7.	Save the CFG file
8.	Search for “ZapReport.cfg“ and open it for editing
9.	Search for the entry “Folder“ and replace the content with the path to the folder, where you want to save the reports. If there is a backslash in your path, then replace it with two backslashes, e.g. if you want to save your reports at “.\Reports“ use “.\\\\Reports“.
10.	Search for entry “Culture“ and set your right culture. The first part is for the language (currently you can enter “en“ or “de“, the second is for the regional settings. E.g. “de-CH“ or “en-US“ would be valid entries.
11.	Search for entry “DoNotPrintPTVsWith“ and add all elements, that you don’t want to have in the list of PTVs. E.g [“Physics“, “Temp“] will exclude all PTVs, that contain the text “Physics“ or “Temp“ in their name. The text is checked case insensitive.
12.	Search for entry “DoNotPrintVOIsWith“ and add all elements, that you don’t want to include in the list of VOIs. E.g [“Body“, “Tune“] will exclude all VOIs, that contain the text “Body“ or “Tune“ in their name. The text is checked case insensitive.
13.	Search for entry “StructureForVolumes“ and replace the entry with the name of the structure you want to use to calculate the V10 and V12 volumes. Normally this is „Brain“.
14.	Save the CFG file

## Using
1.	Start “ZapReport.exe“
2.	If you didn’t provide a username and/or password in the CFG file, you are asked to provide this
3.	Main screen should open. Looks like this

 ![image](https://github.com/ZapAddOns/ZapReport/assets/130468140/0816c1fb-67a1-4d7c-afbf-bbb60694b29f)


4.	Select an active patient. If the patient is already archived, check “Archived“ and you will see all patients.
5.	Select a plan for this patient
6.	If this plan has already the status “Partially delivered“ or “Fully delivered“ you can select a fraction. If you need all fractions in you report, select “All“.
7.	Select the physician, who signed the plan (only, if you want to print “Signs“). Additionally you can set the date of the plan authorization (normally, it is set already) and if a sign for this person is provided in the “Signs“ folder, select the check box at the end.
8.	Select the physicist, who signed the plan (only, if you want to print component “Signs“). Additionally you can set the date of plan authorization (normally, it is set already) and if a sign for this person is provided in the “Signs“ folder, select the check box at the end.
9.	Select, the type of report. When new, there are the options “Planning report“ and “Treatment report“. If you select a type, the content of the list boxes below will change.
10.	Select which components you want to print. Left list box shows the components that are excluded, right list box shows, which components are included. Select an entry in one of the list boxes and move it with the arrow buttons from box to box or up and down. The components are printed in the order of the right list box.
11.	If you want to add a page break, then insert it with the “PB“ button and move it with the up and down arrows at the right position
12.	If you want to save this for the next time: use the “S“ button. 
13.	Press “OK“ button
14.	The created report is saved in the report folder provided in the “ZapReport.cfg“ file as a PDF/A file

## Further settings
Create additional report types
In the config file “ZapReport.cfg“ you can add new report types  In the list of “ReportTypes“ there is an entry for each report type. This report type consists of
-	“Titel“: Shown in the selection of „Print“ in the dialog and as caption of the report
-	“Filename“: Used as mask to create the filename to save the report. There are some special placeholders, that are replaced on the fly: “{MedicalId}“, “{PatientFirstName}“, “{PatientMiddleName}“, “PatientLastName}“ and “{PlanName}“. This can be combined with other text, e.g. “{MedicalId} - {PlanName} – SummaryReport“. Extension “.pdf“ is added automatically.
-	“Components“: List of components printed with this report. Best to select them inside the dialog and then save the list into the config file with the “S“ button. 

## Problems
If you have any problems while using ZapReport, send an e-mail to d.weltz@snrc.ch. Please add the version, a screenshot and the “.log” file of your system (can be found in the app folder).

## Libraries
The following libraries are used to create ZapReport
- ZapTranslation: https://github.com/ZapAddOns/ZapTranslation (LGPL)
- ZapClient: https://github.com/ZapAddOns/ZapClient (LGPL) 
-	Zap zsClient: https://zapsurgical.com/de/ (Closed source)
-	ScottPlot: https://scottplot.net/ (MIT license)
-	QuestPDF: https://www.questpdf.com/ (MIT license)
-	SkiaSharp: https://github.com/mono/SkiaSharp (MIT license)
-	Harfbuzz: https://github.com/harfbuzz/harfbuzz (MIT license)
-	Newtonsoft Json.NET: https://www.newtonsoft.com/json (MIT license)
