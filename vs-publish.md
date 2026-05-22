# Publicação no Vintage Story Mod DB — Unforgettable

Documentação da consulta sobre preenchimento da página do mod no [Vintage Story Mod Database](https://mods.vintagestory.at/).

- **Página do mod:** [https://mods.vintagestory.at/show/mod/50588](https://mods.vintagestory.at/show/mod/50588)  
- **Formulário de edição:** [https://mods.vintagestory.at/edit/mod/?assetid=50588](https://mods.vintagestory.at/edit/mod/?assetid=50588)  
- **Referências:** [Category:Modding](https://wiki.vintagestory.at/Category:Modding), [Modding:Modinfo](https://wiki.vintagestory.at/Modding:Modinfo), [Modding:Mod Packaging](https://wiki.vintagestory.at/Modding:Mod_Packaging)

---

## Pergunta

> Consultando a documentação de MOD do jogo Vintage Story ([https://wiki.vintagestory.at/Category:Modding](https://wiki.vintagestory.at/Category:Modding)), me ajude a preencher as informações faltantes na página desse MOD ([https://mods.vintagestory.at/edit/mod/?assetid=50588](https://mods.vintagestory.at/edit/mod/?assetid=50588)).

---

## Resposta

### Estado atual da página


| Campo     | Situação                                     |
| --------- | -------------------------------------------- |
| Nome      | `Unforgettable` — OK                         |
| Status    | **Draft** — precisa mudar para **Published** |
| Releases  | **Nenhum** — falta enviar o `.zip`           |
| Descrição | Vazia na página pública                      |
| Tags      | Provavelmente vazias                         |


O `modinfo.json` ainda descreve apenas o forno de barro; o mod já cobre **forno de barro**, **panela no fogão** e **cadinho no fogão**.

---

### Metadados básicos (formulário de edição)


| Campo                         | Valor sugerido  |
| ----------------------------- | --------------- |
| **Mod ID**                    | `unforgettable` |
| **Name**                      | `Unforgettable` |
| **Type**                      | `Code`          |
| **Side**                      | `Client`        |
| **Authors**                   | `Mintir4`       |
| **Game version** (no release) | `1.22.2`        |
| **Mod version** (no release)  | `1.0.0`         |


---

### Summary (texto curto da listagem)

```
Client-side QoL mod: HUD timer icons and repeating alarms when clay oven baking, firepit cooking pots, or crucibles finish.
```

---

### Description (aba Description — Markdown)

```markdown
## Unforgettable

Never miss the perfect moment again. **Unforgettable** is a lightweight **client-side** code mod that watches your cooking and smelting stations and alerts you when food or metal is ready — before it burns or sits forgotten.

### Features

- **HUD timer icons** on the left side of the screen while something is in progress
- **Blink speed increases** with progress (0×/s up to 2×/s)
- **Repeating alarm sound** every 5 seconds when cooking/smelting is complete
- Alarm and icon **stop automatically** when you remove the finished item
- **Progress is preserved** if the fire goes out mid-cook (icon stays visible)

### Supported stations

| Station | What is tracked | HUD icon |
|---------|-----------------|----------|
| **Clay oven** | Bread and pie reaching `-perfect` from `-partbaked` | Oven timer |
| **Firepit + cooking pot** | Meals and `CooksInto` recipes (e.g. rendered fat) | Cooking pot timer |
| **Firepit + crucible** | Ore smelting in a crucible | Crucible timer |

### How it works

Harmony patches intercept client-side sync updates from `BlockEntityOven` and `BlockEntityFirepit`. No server install required — works on any world where you have the mod locally.

### Requirements

- **Vintage Story 1.22.2+**
- **Client only** — no server-side component needed
- Depends on vanilla `game` 1.22.2

### Credits

- **Code** developed with [Cursor Composable](https://cursor.com)
- **Images** (HUD timer icons and mod icon): generated with Gemini nanobana
- **Oven/firepit alarm sound** (`oventialarm.ogg`): adapted from a sample on [FreeSound](https://freesound.org/)
```

---

### Tags sugeridas

Marcar no Mod DB:

- **QoL**
- **HUD**
- **UI**
- **Food**
- **Oven**
- **Firepit**
- **Sound**
- **Client**
- **Code**

Opcionais: **Utility**, **Tweak**, **Crafting**

---

### Logo / modicon

Não há `modicon.png` no projeto. Segundo a [wiki de empacotamento](https://wiki.vintagestory.at/Modding:Mod_Packaging), o jogo usa `modicon.png` na raiz do zip.

**Sugestão:** usar `assets/unforgettable/textures/oven_timer_inverted_transparent.png` como base, redimensionar para ~256×256 e enviar como logo no Mod DB **e** incluir `modicon.png` na raiz do zip.

---

### Release (aba Files)

1. Compilar e empacotar o mod (estrutura conforme [Modding:Mod Packaging](https://wiki.vintagestory.at/Modding:Mod_Packaging)):

```
Unforgettable-mv_1.0.0-gv_1.22.2.zip
├── modinfo.json
├── modicon.png          ← recomendado
├── Unforgettable.dll
└── assets/
    └── unforgettable/
        ├── sounds/
        │   ├── oventialarm.ogg
        │   └── crucible_sound.wav
        └── textures/
            ├── oven_timer_inverted_transparent.png
            ├── cooking_pot_timer_inverted_transparent.png
            └── crucible_timer_inverted_transparent.png
```

1. Fazer upload na aba **Files**
2. **Mod version:** `1.0.0`
3. **Game version:** `1.22.2`

**Changelog sugerido para 1.0.0:**

```markdown
Initial release.

- Clay oven alarm for bread and pie (perfect bake detection)
- Firepit cooking pot timer and alarm (meals and CooksInto recipes)
- Crucible smelting timer and alarm
- HUD icons with progress-based blinking
- Repeating alarm every 5 seconds until item is removed
```

---

### Publicar

No formulário de edição, mudar de **Draft** para **Published**. Rascunhos só aparecem por link direto.

---

### Atualizar também o `modinfo.json`

A descrição in-game ainda está desatualizada. Sugestão:

```json
"description": "Client-side HUD timers and repeating alarms for clay oven baking, firepit cooking pots, and crucible smelting"
```

Campos opcionais úteis ([Modding:Modinfo](https://wiki.vintagestory.at/Modding:Modinfo)):

```json
"website": "https://mods.vintagestory.at/show/mod/50588",
"iconPath": "modicon.png"
```

---

### Checklist final

1. Preencher **Summary** e **Description**
2. Adicionar **tags**
3. Enviar **logo**
4. Fazer upload do **release 1.0.0** para game **1.22.2**
5. Mudar status para **Published**
6. Atualizar `modinfo.json` para bater com a página

