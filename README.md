# Erasing language barriers in international BIM collaboration.

**Revit Translator** is a free add-in for Autodesk Revit, aimed at BIM teams working on international projects that require translation of models or drawings into another language.

It simplifies the translation pipeline to **100+ languages** by automatically extracting text from all user-modifiable text properties, translating it via a third-party service, and updating them back in Revit. This plug-in uses the API provided by **DeepL** — industry leader in accurate technical machine translations.

> Supports Revit 2023 · 2024 · 2025 · 2026

---

![github-poster](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/34d3c2e5-4887-45ca-9d2a-754ee7dc71a2)

---

# Setup

## 1. Download & Install

1. Go to the [latest GitHub release](../../releases/latest).
2. Download the `.msi` file for each Revit version you're using.
3. Run the file. Windows will show a standard security warning — click **More Info → Run Anyway**.

<img width="392" height="311" alt="Windows security prompt" src="https://github.com/user-attachments/assets/b0c2f399-25cc-4c12-8360-8ab2c4e51a93" />

4. Start Revit and click **Always Load**.

![Revit load prompt](https://github.com/Krachkovskii/RevitTranslator/assets/117347760/48934b38-dfbd-4b14-bbfd-de40818c45d5)

---

## 2. Set Up a DeepL Account & API Key

To use the add-in, you need a DeepL API account. Two tiers are available:

| Tier | Limit | Cost |
|------|-------|------|
| **Free** | 500,000 characters/month | Free (card required) |
| **Pro** | Unlimited | Pay-as-you-go |

1. Go to [deepl.com/pro#api](https://www.deepl.com/en/pro#api) and register an account.
2. Set up billing — required even for free accounts. You won't be charged on the free tier.
3. Create an API key: **Account → API keys & limits → Create key**.

> ⚠️ Payment cards from some countries may not be accepted by DeepL. Check their website for details.

---

## 3. Configure the Add-In in Revit

1. Open the **Add-Ins** tab in Revit.
2. Click **Translator → Settings**.
3. Paste your API key. Free or Pro tier is detected automatically.
4. Select your **target language**. Source language is optional — it can be auto-detected.
5. Click **Save Settings**.

<img width="500" alt="Settings window" src="https://github.com/user-attachments/assets/ea6d8217-2f13-412a-b023-f8cc3da684f2" />

> From the Settings window you can also access previous translation reports, or navigate to the GitHub Issues page if something goes wrong.

---

## Update & Uninstall

- **Updates** are automatic — the add-in checks for new releases while Revit is running and installs them silently after Revit closes.
- **Uninstall** via **Control Panel**, like any other application.

---

# How to Use

## Price

The plug-in itself is **free**. Translation costs depend on your DeepL plan:

- **Free tier** — 500,000 characters/month. As a rough guide, that's about the length of *The Great Gatsby* translated every week. How far it goes in a Revit model depends heavily on how much text your model contains.
- **Pro tier** — No monthly cap. Pay-as-you-go pricing at [deepl.com/pro#api](https://www.deepl.com/en/pro#api).

---

## Translation Modes

The add-in offers four modes. Choose the one that fits your workflow:

<img height="250" alt="Translation modes UI" src="https://github.com/user-attachments/assets/05183b39-0e2d-49a3-aa6f-2b5ef7cf34d5" />

### Selection
Translates all currently selected elements. Works with elements picked in the viewport as well as in the Project Browser (Views, Family Types, etc.).

### Categories
Select one or more Revit categories (Doors, Text Notes, Floor Tags, etc.) and translate all matching elements in the model. The add-in will show you the total count of translatable elements when you update your selection.

> **Note:** The actual number of translation calls is roughly 10× the element count, since each element has multiple text properties.

### Sheets
Select individual sheets or sheet collections *(collections available in Revit 2025+)*. Translates all elements on the sheet — title block, annotations — as well as everything visible in placed viewports.

### Model
Collects and translates every user-editable element in the model. The most thorough option, and the most time-consuming.

---

## Features

### Cancellation & Undo

Translations can be stopped at any point by clicking **Cancel** in the progress window. You can then choose to apply the completed portion, or discard everything.

Already-applied translations can be fully reversed with a single **Ctrl+Z** — the entire session is committed as one transaction.

### Workshared Models

It is recommended to run translations on a **detached copy** of the model to avoid conflicts. If working in a workshared environment, ensure all relevant elements are editable before starting.

### Translation Speed

DeepL rate-limits API calls, so the add-in paces requests accordingly. Expect **3–5 translations per second**, with occasional short pauses — this is normal.

### Progress & Reports

A **Progress window** shows remaining translations and running character usage in real time.

After each session, a **report is saved automatically**, containing:
- Session summary
- Each translated entry with original text, translation, and Revit element ID

Reports can be opened from the Progress window or accessed later via **Settings → Open Reports Folder**.
Default location: `%APPDATA%\Autodesk\ApplicationPlugins\RevitTranslator\Reports`

<img width="600" alt="Progress window" src="https://github.com/user-attachments/assets/200873c6-a4a6-44c3-80ef-aa66c665f25b" />

### ~~Black/White Lists~~ *(not yet supported)*

The add-in currently does not support excluding specific elements or categories from translation. As a workaround, use **Selection** or **Categories** mode to translate only what you need.

---

## What Gets Translated

The add-in handles a range of Revit element types:

| Element type | What's translated |
|---|---|
| **Text Notes & Text Elements** | Text content |
| **Views** | View name |
| **Schedules** | Column headers |
| **Dimensions** | Above / below / prefix / suffix / value override |
| **Title Blocks** | All text elements inside the family |
| **Families, Types, Instances** | Name + all user-modifiable string parameters |

> The add-in will skip values containing characters that Revit treats as reserved or invalid (such as `< > | :`). Revit will display the full list if this occurs.

---

## Technical Details

- Tested on **Windows 11**, Revit **2023** and **2025**
- Designed to support Revit **2023–2026**
- Compatibility with other add-ins is not guaranteed — conflicts may occur if another plug-in uses a different version of a shared package

---

# Afterword

Have feedback, a proposal, or just want to say hello? Reach out on [LinkedIn](https://www.linkedin.com/in/ilia-krachkovskii/) or by [email](mailto:i.krachkovskii@gmail.com).

<details>
<summary>Background & motivation</summary>

Years ago, I was working in an architectural studio on a project in another country. All drawings had to be handed off in the client's language — a perfectly standard practice.

The prototype for this plug-in was a Dynamo script that made the process bearable: extract text from elements into an Excel file (tracking Parameter IDs) → hand it to translators → proofread the results → feed the Excel back into Dynamo → update parameter values automatically.

It was a time-saver, but eventually I decided it would be a good challenge to build something more polished — and to learn .NET development along the way.

</details>

### A note on support

This is a free, open-source plug-in maintained in my spare time. I cannot guarantee prompt bug fixes, new features, or indefinite maintenance.

If it's useful to you, a kind word goes a long way — share it, write a post, leave a comment, or send a quick message on LinkedIn.

---

> **Legal notice:** Always work from backups and local files. Use this plug-in at your own discretion.
