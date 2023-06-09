# QuestionnaireX

This application is a generic questionnaire system based on Windows Forms.
It runs on every system with Microsoft Windows and .NET Framework installed.
You can easily create new types of questions as a Windows Form yourself.
See the `Demo/` folder for examples of each question type.

The project relies on the FileHelpers package from NuGet, which is not contained in this repository.
It can be restored in Visual Studio by selecting "Tools" -> "NuGet Package Manager" -> "Package Manager Console" and then clicking "Restore".

The following question types have been implemented until now:

* **Instruction**
This form presents the given experiment instruction text to the participant.
They have to confirm that they understood the instruction to continue.

* **Scale**
Below a given question, there is a simple slider that can be moved to every integer between a given minimum and maximum value.
The participant will see which value is currently chosen.
You can specify one descriptor for each extreme.
The return value is the chosen integer.

* **ScaleVAS**
Below a given question, there is a simple slider that can be moved between 0 and 1000.
The participant does not see which value is currently chosen.
You can specify one descriptor for each extreme.
The return value is a floating point number between 0 and 1.

* **Text**
Below a given question, there is a simple multiline text input that can be used to enter some text.

* **Buttons**
Below a given question, between 1 and 10 buttons with individual labels can be shown.
The return value is the button ID from the input file.
(Not displayed to the participant.)

* **ButtonsImage**
Below a given question, there can be shown between 1 and 10 buttons with individual labels.
The return value is the button ID from the input file which the participant does not see.
Right next to the buttons, an image with at maximum 767 pixels height and width can be displayed for each question.
Larger images will be scaled down.

**All input and question files must be encoded using either Windows-1252 (Notepad++ calls it "ANSI") or UTF-8!**

As long as you save out the input file in **semicolon-delimited** CSV format, you can for example even use Excel for creating and editing it, which makes it much easier to keep track of which data cell belongs to which column.
To tell Windows that data cells should be delimited by semicolons, make sure the field "List separator" under "Region and Language" -> "Additional settings..." is set to a semicolon.
