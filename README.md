## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

# [CS2] Auto-Downloader-GoldKingZ (1.0.1)

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

## ⌨️ Commands

| Command | Description | Usage |
|---------|-------------|-------|
| `gkz_update <workshopid>` | Force Update/Download A Specific Workshop To The Server | `gkz_update 12345678` |
| `gkz_download <workshopid>` | Force Update/Download A Specific Workshop To The Server | `gkz_download 12345678` |

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
| `Workshop_These_Only` | Precache **ONLY** listed folders/files found within Workshop VPKs. |
| `Workshop_All_Exclude_These` | Precache **ALL** from Workshop VPKs **EXCEPT** the listed items. |
| `Custom_Include` | **Direct Precache**: Use this for CS2 / Workshop / Any |
| *Empty / Missing* | If no Workshop rules are set, the system will precache **Everything** in Workshop VPKs. |

## Map Matching & Priority
The system searches for a configuration match in this order:
1. **Exact Match**: Full name (e.g., `de_dust2`).
2. **Prefix Match**: Matches maps starting with the key (e.g., `de_`). **MUST end with `_`**.
3. **Global Match**: Uses `*` if no specific or prefix match is found.

## Map Configurations
| Map Name | Strategy Used | Paths / Files |
|-------------|-------------|-------------|
| `*` | `Custom_Include` | `sounds/player/taunt_clap_01.vsnd` |
| `de_` | `Custom_Include` | `soundevents2/game_sounds_ui.vsndevts` |
| `de_dust2` | `Workshop_These_Only` | `models/dev/`, `materials/dev/` |
| `cs_office` | `Workshop_All_Exclude_These` | `scripts/weapons.vdata` |

</details>


## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

### [1.0.1]
- Added Prefix `de_` `cs_`  
- Added Custom_Include 
- Rename These_Only To Workshop_All_Exclude_These
- Rename All_Exclude_These To Workshop_All_Exclude_These

### [1.0.0]
- Initial plugin release

</details>

---
