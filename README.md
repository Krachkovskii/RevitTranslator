# Erasing language barriers in international BIM collaboration.

Revit Translator is a free add-in for Autodesk Revit, aimed at BIM teams working on international projects that require translation of models or drawings into other language.

It simplifies translation pipeline to 100+ languages by automatically extracting text from all user-modifiable text properties, translating it via third-party service, and updating them back in Revit. This plug-in uses API provided by DeepL - industry leader in accurate technical machine translations. Plug-in supports Revit versions from 2023 to 2026.

---

![github-poster](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/34d3c2e5-4887-45ca-9d2a-754ee7dc71a2)

---

# Setup
### 1. Download & install plug-in
1. Go to the latest GitHub release.
2. Download .msi file for each Revit version that you're using.
4. Run the file. Windows will show a standard warning: click `More Info -> Run Anyway`.
   
<img width="392" height="311" alt="image" src="https://github.com/user-attachments/assets/b0c2f399-25cc-4c12-8360-8ab2c4e51a93" />

6. Start Revit, click `Always Load`.

    ![image](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/48934b38-dfbd-4b14-bbfd-de40818c45d5)

### Set up DeepL account & API key
To use the addin, you need to set up an account with DeepL. There are two tiers: Free and Pro. Free tier has a limit of 500 000 characters/month, while Paid tier offers unlimited pay-as-you-go translations. Please refer to DeepL website for detailed information on how to create account and create an API key. 
Overall steps are:

1. Go to DeepL website ([www.deepl.com](https://www.deepl.com/en/pro#api)) and register an account.
2. Set up your billing; it's necessary even for free accounts. You won't be charged.
3. Create an API key: `Account -> API keys & limits -> Create key`. It can be copied later.

### 2. Set up translation settings in Revit
1. The app is located in Revit's default `Add-Ins` tab.
2. Click on the `Translator` button, then on `Settings`.
3. Paste your API key. Paid or Free tier will be determined automatically.
4. Select target language. Source language is optional and can be auto-detected.
5. Click `Save Settings`.

<img width="500" alt="image" src="https://github.com/user-attachments/assets/ea6d8217-2f13-412a-b023-f8cc3da684f2" />
From the settings window, you can also access reports for previous translations, or go to my LinkedIn or on GitHub Issues page if something goes wrong.

## Update
The app automatically checks for updates when Revit is running. If there's a new release, it will silently download the latest version after Revit is closed.

## Uninstall
Add-in can be uninstalled from `Control Panel` like any other application.

# How to use

## Price
The plug-in itself is free to use. DeepL API provides two tiers: Free and Pro. 
- Free is limited to 500 000 characters per month; that's an equivalent of "the Great Gatsby" being translated every week. Model-wise, it can be very different and depends heavily on text information density in your model.
- Paid tier does not have usage cap; instead, it has a pay-as-you-go scheme. Please refer to https://www.deepl.com/en/pro#api for pricing.

## Usage modes
The app has four modes of translation: `Current selection`, `Categories`, `Sheets` and `Model`.

<img height="250" alt="image" src="https://github.com/user-attachments/assets/05183b39-0e2d-49a3-aa6f-2b5ef7cf34d5" />


#### Selection
The app gets all currently selected elements and translates them. You can select elements in the viewport, as well as in the project browser, such as Views or Family Types.
#### Categories
You can select any number of Revit Categories, such as Doors, Text Notes, Floor Tags etc., and translate all elements of these categoories in the model.
When you update your selection of categories, the app will show you the total number of translatable elements. NB: actual number of translations will be ~10x higher, since each element contains at least several translatable properties.
#### Sheets
You can select sheets or sheet collections (available in Revit 2025+). It will translate all elements on the sheet (TitleBlock, annotations, etc.) and all elements visible in Viewports placed on said sheet.
#### Model
The app collects all user-editable elements in the model and translates them all. The lazy approach, most time- and resource-consuming.

## Features

### - Translation cancellation
Translations can be stopped mid-way. To do this, click "Cancel" in the progress window. 
You can opt to update the model with completed translation by pressing the button in the same place, or just close the window to leave everything as-is.

To cancel already performed translation, simply click "undo" button in Revit -- all translations are done within a single transaction.

### - Shared model usage
To avoid any unexpected behavior, it is recommended to perform translations in a detached model. Or at least ensure that all elements can be modified.

### - Translation speed
DeepL limits request rate to their endpoints, so I had to slow down calls using a semaphore. You can expect speed of 3-5 translations per second, with occasional pauses - don't panic, let it cook.

### - Progress & Reports
When translation is in progress, a Progress window is shown. You can see how many translations are left and how your usage goes up.
After a successful translation, add-in will save a report. It will include overall session information, as well as detailed information on each translated entry, including original text, translation and element ID. You can open report from progress window after model has finished updating. You can also open folder with reports from Settings window. Generally, reports will be saved in `%APPDATA%\Autodesk\ApplicationPlugins\RevitTranslator\Reports`.

<img width="600" alt="image" src="https://github.com/user-attachments/assets/200873c6-a4a6-44c3-80ef-aa66c665f25b" />


### [not supported] Black/white lists
Currently, add-in does not support exclusion of specific elements or categories. For this, you can try selecting only specific categories, or manually selecting elements you want to translate using Selection mode.

### - Translation logic
The plug-in handles nuances of many kinds of Revit elements. Important notice: all elements selected for translation will also be processed as "base" elements.
Plug-in will not update some values with characters that Revit considers reserved or invalid, such as `<>|:` (that's not all - Revit will show you the full list)

* **Text elements**: `TextElement` and `TextNote`. The `text` property will be translated.
* **Views**
* **Schedules**: schedule column headers.
* **Dimensions**: all overrides - `above`, `below`, `suffix`, `prefix` and `value override`. Single- and multi-segment dimensions are supported.
* **Titleblocks**: all `TextElements` *inside* a titleblock family will be translated.
* **Elements**: Element name, as well as **all** user-modifiable string-based parameters. This is the base translation, always applied to all elements.
  * **Families**
  * **Family types**
  * **Family instances**

## Technical details
The add-in was tested on Windows 11 and in Revit versions 2023 and 2025. It is designed to work in all Revit versions from 2023 to 2026.
Plug-in has not been tested for compatibility with other plug-ins, and may not load if they are using other versions of packages that Revit Translator requires.

# Afterword
Do you have any feedback, proposals, inquiries or offers? Feel free to write me on [LinkedIn](https://www.linkedin.com/in/ilia-krachkovskii/), or send me an [email](mailto:i.krachkovskii@gmail.com). 

#### Justification
Years ago, I was working in an architectural studio on a project in another country. All drawings had to be handed off in client's language -- a perfectly expected practice.
Prototype for this plugin was a Dynamo script that made the process more bearable: process necessary elements, extract text -> put it into an Excel file, keeping track of Parameter ID this text belonged to -> hand this table over to professional translators -> proofread everything: company decided to hire non-technical translators, as a native speaker I could check out their work and adjust result as necessary -> upload this Excel file back to Dynamo -> automatically update values for each parameter. 
This approach was a time-saver, but eventually I decided that it's a nice challenge for myself to learn more about development for .NET.

#### Appreciation
This is a free and open-source plug-in that I develop in my free time. I cannot guarantee prompt bugfixes, implementation of new feature or maintenance in an indefinite future. 
If you like it, a kind word goes a long way: share it, write a post, comment, or send me a simple direct message on LinkedIn.

#### Legal notice
Remember using back-ups and local files. Use plug-in at your own discretion.
