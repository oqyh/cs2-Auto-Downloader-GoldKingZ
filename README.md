---
<h2 align="center">.:[ Community | Support ]:.</h2>
<p align="center">
  <a href="https://discord.com/invite/U7AuQhu">
    <img src="https://img.shields.io/badge/Discord-Join-5865F2?style=for-the-badge&logo=discord&logoColor=white" />
  </a>
  <a href="https://ko-fi.com/goldkingz">
    <img src="https://img.shields.io/badge/Ko--fi-Support-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white" />
  </a>
</p>

---


# [CS2] Auto-Downloader-GoldKingZ (1.0.2)

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

## 🛠️ `config/config.json`

<details open>
<summary><b>Main Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |
|----------|-------------|--------|----------|
| `ForceDownloadMissing` | Force Download Missing Workshops If Not Found In The Server | `true`/`false` | - |
| `Precache_Filter` | Per-Map Precache Rules (See Precache Filter Examples Below) | List of rules<br>`[]` = Precache All | - |
| `EnableDebug` | Enable Debug Mode | `true`/`false` | - |

</details>

<details>
<summary><b>Precache Filter</b> (Click to expand 🔽)</summary>

### Precache Logic

| Strategy | Description |
|----------|-------------|
| `Workshop_These_Only` | Precache **ONLY** listed folders/files found within Workshop VPKs. |
| `Workshop_All_Exclude_These` | Precache **ALL** from Workshop VPKs **EXCEPT** the listed items. |
| `Custom_Include` | **Direct Precache**: Use this for CS2 / Workshop / Any. |
| *Empty `Precache_Filter`* | If the filter list is empty, the system will precache **Everything** in Workshop VPKs by default. |

### Map Matching & Priority

The system searches for a configuration match in this order:
1. **Exact Match**: Full map name (e.g., `de_dust2`).
2. **Prefix Match**: Matches maps starting with the key (e.g., `de_`). **MUST end with `_`**. Longest prefix wins.
3. **Fallback Match**: Uses `ANY` (or an empty `MapName`) if no exact or prefix match is found.

> [!NOTE]
> If `Precache_Filter` contains rules but none match the current map (no exact, prefix, or `ANY` fallback), the plugin will log a warning and skip precaching for that map.

### Example Configuration

```json
"Precache_Filter":
[
  {
    "MapName": "ANY",
    "Custom_Include":
    [
      "sounds/player/taunt_clap_01.vsnd"
    ]
  },
  {
    "MapName": "de_",
    "Custom_Include":
    [
      "soundevents2/game_sounds_ui.vsndevts"
    ]
  },
  {
    "MapName": "de_dust2",
    "Workshop_These_Only":
    [
      "models/dev/",
      "materials/dev/"
    ]
  },
  {
    "MapName": "cs_office",
    "Workshop_All_Exclude_These":
    [
      "scripts/weapons.vdata"
    ]
  }
]
```

### Map Configurations

| Map Name | Strategy Used | Paths / Files |
|----------|---------------|---------------|
| `ANY` | `Custom_Include` | `sounds/player/taunt_clap_01.vsnd` |
| `de_` | `Custom_Include` | `soundevents2/game_sounds_ui.vsndevts` |
| `de_dust2` | `Workshop_These_Only` | `models/dev/`, `materials/dev/` |
| `cs_office` | `Workshop_All_Exclude_These` | `scripts/weapons.vdata` |

</details>

## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

### [1.0.2]
- Migrated `Precache_Filter` from `precache_config.json` into the main `config.json`
- Auto-removes legacy `precache_config.json` on plugin load if found
- Making EnableDebug False By Default
- Fix Console Color On Windows And Lunix

### [1.0.1]
- Added Prefix `de_` `cs_`  
- Added Custom_Include 
- Rename These_Only To Workshop_All_Exclude_These
- Rename All_Exclude_These To Workshop_All_Exclude_These

### [1.0.0]
- Initial plugin release

</details>

---
