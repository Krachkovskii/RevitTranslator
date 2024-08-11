# International projects in BIM just got easier.
Revit Translator is an add-in for Autodesk Revit that performs automated translation and update of text data in your Revit models. Use it to translate views, schedules, annotations and much more.

It uses DeepL API to perform quick and precise translations of whole Revit projects within minutes.

---

![github-poster](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/34d3c2e5-4887-45ca-9d2a-754ee7dc71a2)

# How to install
Starting from v0.2, releases feature a simple installer that takes care of placing all necessary files.
### Installation steps
1. Go to GitHub Releases (on the right side of this screen).
2. Download .msi file from the release.
3. Run it. Select Revit versions you're planning to use, from 2021 to 2024. Click on all "next" and "finish" buttons you see.
4. Start Revit, click `Always Load` or `Load Once` (this window will be shown every time ).

    ![image](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/48934b38-dfbd-4b14-bbfd-de40818c45d5)


## How to set up
To use the addin, you need to set up a DeepL Pro account. Free account has a limit of 500 000 characters/month, while Paid tier offers unlimited paid translations.
In Revit, the app says individual configuration for each computer and each Revit version.
### DeepL setup steps
1. Go to [DeepL site](www.deepl.com), register an account;
2. Set up your billing; it's necessary even for free accounts. You won't be charged, but unfortunately, bank cards of some countries may not be permitted;
3. Create an API key. It can be copied later.

### Revit setup steps
1. The app is in Revit's default `Add-Ins` tab;
2. Click on the `Translator` button, then on `Settings`;
3. Set up your API key (and specify if you have Free or Paid plan) and translation languages, click `Save Settings`.

    ![image](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/910ef370-b7b9-4b71-a11e-69cd0c200b6a)

Settings configurations are saved in this path: `%appdata%\Autodesk\Revit\Addins\(RevitVersion)\RevitTranslatorAddin\Settings\settings.json`. You can copy this file to another computer or Revit version instead of manually filling translation setup.

# How to use  
The app has three modes of translation: current selection, categories and whole model.
### Current selection
The app gets all currently selected elements and translates them. You can select elements in the viewport, as well as elements in the project browser, such as Views or Family Types.
### Selected Categories
You can select any number of Revit Categories, such as Doors, Text Notes, Floor Tags etc., and translate all elements of these categoories in the model.
When you update your selection of categories, the app will show you the total number of translatable elements.
### Whole model
The app collects all user-editable elements in the model and translates them all. This mode comes with a warning that tells you that there's just so many elements in the model. You have to confirm that you actually want to translate them all... Just a little safety feature.

Translations can be stopped mid-way. To do this, click "Stop translation" in the progress window. Completed translations will still be applied. If you don't need them, just hit `ctrl+Z`.

## How to uninstall
1. Open Windows' Run command (Win + R) and insert the following path: `%appdata%\Autodesk\Revit\Addins`, click Enter.
2. Open the corresponding Revit version folder.
3. Make sure Revit is not running.
4. Remove the folder `RevitTranslatorAddin` and manifest file `RevitTranslatorAddin.addin`.
5. Go to `Control Panel` -> `Programs and Features`. Select `Revit Translator` and click "uninstall".

## Roadmap
There are lots of things I would love to improve. Eventually I will deal with some of them in my free time.
### UI:
* Introduce "Black list" (or is it a white list?) for parameter names and values to prevent their translation.
* Add glossaries.
* Maybe add a "translator" mode with a dockable pane, where you can just translate stuff.
### Revit:
* Increase number of translatable element types.
* Add translation of parameter names (at least global and user-added from downloadable families).
### Application:
* Add at least primitive logging.

## Technical details
The add-in was tested on Window 10 & 11 and in Revit versions 2022-2023. It should work in all Revit versions from 2021 to 2024.

It uses asynchronous translation methods, so the total number of translations in the progress window that you see at the beginning is not final.

### Currently, the following elements are translated:
* Elements, i.e. system and downloadable families, views, etc.: Name of the element, all text-based Instance and Type parameters
* Dimensions: all override values
* TextNotes: text contents
* Schedules: titles and field headers

# Afterword
Do you have any feedback, proposals, inquiries or offers? Feel free to write me on [LinkedIn](https://www.linkedin.com/in/ilia-krachkovskii/). 

I am open to collaborations for BIM development and consulting. My expertise includes 4+ years with international teams on top-level projects, including NEOM, JetBrains HQ and SberCity, where I provided technical assistance on unique solutions for both projects and studios.
