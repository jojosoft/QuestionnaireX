# QuestionnaireX

This application is a generic questionnaire system based on Windows Forms. You can easily create new types of questions as a Windows Form yourself. It runs on every system with Microsoft Windows and .NET Framework installed. The project relies on the FileHelpers package from NuGet, which is not contained in this repository and can be restored in Visual Studio by selecting "Tools"->"NuGet Package Manager"->"Package Manager Console" and then clicking on "Restore".

The following question types have already been implemented until now:
* **Instruction**
This form presents the given experiment instruction text to the participant. They have to confirm that they understood the instruction to continue.
* **Scale**
Below a given question, there is a simple slider that can be moved to every integer between a given minimum and maximum value. The participant will see which value is currently chosen. You can specify one descriptor for each extreme. The return value is the chosen integer.
* **ScaleVAS**
Below a given question, there is a simple slider that can be moved between 0 and 1000. The participant does not see which value is currently chosen. You can specify one descriptor for each extreme. The return value is a floating point number between 0 and 1.
* **Text**
Below a given question, there is a simple multiline text input that can be used to enter some text.
* **Buttons**
Below a given question, there can be shown between 1 and 10 buttons with individual labels. The return value is the button ID from the input file which the participant does not see.
* **ButtonsImage**
Below a given question, there can be shown between 1 and 10 buttons with individual labels. The return value is the button ID from the input file which the participant does not see. Right next to the buttons, an image with at maximum 767 pixels height and with can be displayed for each question. Larger images will be scaled down.