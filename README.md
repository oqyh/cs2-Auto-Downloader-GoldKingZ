## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

# [CS2] Auto-Downloader-GoldKingZ (1.0.0)

Automatically Download/Precaches Addons Depend Map Config + Update Manually For MultiAddonManager

![auto-downloader](https://github.com/user-attachments/assets/96d36978-5446-48b8-8bea-64b0a76b147f)


---

## 📦 Dependencies

[![Metamod:Source](https://img.shields.io/badge/Metamod:Source-2d2d2d?logo=sourceengine)](https://www.sourcemm.net)

[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-83358F)](https://github.com/roflmuffin/CounterStrikeSharp)

[![JSON](https://img.shields.io/badge/JSON-000000?logo=json)](https://www.newtonsoft.com/json) [Included in zip]

[![ValvePak](https://img.shields.io/badge/ValvePak-181717?logo=github&logoColor=white)](https://github.com/ValveResourceFormat/ValvePak) [Included in zip]

---

## 📥 Installation

### Plugin Installation
1. Download the latest `Auto-Downloader-GoldKingZ.x.x.x.zip` release
2. Extract contents to your `csgo` directory
3. Configure settings in `Auto-Downloader-GoldKingZ/config/config.json`
4. Restart your server

---

## ⚙️ Configuration

> [!IMPORTANT]
> **Main Configuration**  
> `../Auto-Downloader-GoldKingZ/config/config.json`  
> **Precache Configuration**  
> `../Auto-Downloader-GoldKingZ/config/precache_config.json`

## 🛠️ `config/config.json`

<details open>
<summary><b>Main Config</b> (Click to expand 🔽)</summary>
  
| Property                            | Description                               | Values                                                                                                                  | Required |
| ----------------------------------- | ----------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- | -------- |
| `ForceDownloadMissing`             | Force Download Missing Workshops If Not Found In The Server | `true`/`false`                                                                                                          | -        |
| `EnableDebug`             | Enable Debug Mode | `true`/`false`                                                                                                          | -        |


</details>

## 🛠️ `config/precache_config.json`

<details open>
<summary><b>Precache Config</b> (Click to expand 🔽)</summary>

## Precache Logic
| Strategy | Description |
|-------------|-------------|
| `Include_These_Only` | Precache **ONLY** the specific files/folders listed |
| `Include_All_Exclude_These` | Precache **ALL** resources except for the ones listed |
| *Empty / Missing* | If no rules are set, the system will precache Everything |

## Map Configurations
| Map Name | Strategy Used | Paths / Files |
|-------------|-------------|-------------|
| `*` | Global | *Default settings for all maps* |
| `de_dust2` | `Include_These_Only` | `models/dev/`, `materials/dev/` |
| `de_mirage` | `Include_All_Exclude_These` | `scripts/weapons.vdata`, `models/goldkingz/...`, `panorama/images/...` |
| `cs_office` | `Include_All_Exclude_These` | `scripts/weapons.vdata` |

**Configuration Notes:**
- The `*` entry acts as the global fallback for any map not explicitly named.
- Folder paths (ending in `/`) will include all assets within that directory.

</details>


## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

### [1.0.0]
- Initial plugin release

</details>

---
