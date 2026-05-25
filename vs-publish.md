# Publicação no Mod DB — Unforgettable

- **Página:** [https://mods.vintagestory.at/show/mod/50588](https://mods.vintagestory.at/show/mod/50588)  
- **Edição:** [https://mods.vintagestory.at/edit/mod/?assetid=50588](https://mods.vintagestory.at/edit/mod/?assetid=50588)

---

## Campos do formulário


| Campo   | Valor                      |
| ------- | -------------------------- |
| Mod ID  | `unforgettable`            |
| Name    | `Unforgettable`            |
| Type    | Code                       |
| Side    | Client                     |
| Authors | `Mintir4`                  |
| Release | mod `1.0.0`, game `1.22.2` |


---

### Summary

```
Client-side HUD timers and alarms for clay oven, firepit pot, and crucible — client-side QoL
```

---

### Text (descrição longa)

Campo **Text** = HTML via TinyMCE (**Tools → Source code**). Não usar Markdown nem `<table>`.

```html
<h2>Unforgettable</h2>

<p>Never miss the perfect moment again. <strong>Unforgettable</strong> is a lightweight <strong>client-side</strong> code mod that watches your cooking and smelting stations and alerts you when food or metal is ready — before it burns or sits forgotten.</p>

<h3>Features</h3>
<ul>
<li><strong>HUD timer icons</strong> on the left side of the screen while something is in progress</li>
<li><strong>Blink speed increases</strong> with progress (0×/s up to 2×/s)</li>
<li><strong>Repeating alarm sound</strong> every 5 seconds when cooking/smelting is complete</li>
<li>Alarm and icon <strong>stop automatically</strong> when you remove the finished item</li>
<li><strong>Progress is preserved</strong> if the fire goes out mid-cook (icon stays visible)</li>
</ul>

<h3>Supported stations</h3>
<ul>
<li><strong>Clay oven</strong> — bread and pie reaching <code>-perfect</code> from <code>-partbaked</code>. HUD icon: oven timer.</li>
<li><strong>Firepit + cooking pot</strong> — meals and <code>CooksInto</code> recipes (e.g. rendered fat). HUD icon: cooking pot timer.</li>
<li><strong>Firepit + crucible</strong> — ore smelting in a crucible. HUD icon: crucible timer.</li>
</ul>

<h3>How it works</h3>
<p>Harmony patches intercept client-side sync updates from <code>BlockEntityOven</code> and <code>BlockEntityFirepit</code>. No server install required — works on any world where you have the mod locally.</p>

<h3>Requirements</h3>
<ul>
<li><strong>Vintage Story 1.22.2+</strong></li>
<li><strong>Client only</strong> — no server-side component needed</li>
<li>Optional: <strong>ConfigLib</strong>, <strong>AutoConfigLib</strong>, and <strong>ImGui</strong> for in-game settings with live HUD preview</li>
</ul>

<h3>Default settings</h3>
<ul>
<li><strong>Left margin:</strong> 2%</li>
<li><strong>Top margin:</strong> 2%</li>
<li><strong>Icon size:</strong> 80 px</li>
<li><strong>Gap between icons:</strong> 10 px</li>
<li><strong>Gap between stations:</strong> 10 px</li>
<li><strong>Clay oven — repeat alarm:</strong> On</li>
<li><strong>Clay oven — show icon when done:</strong> On</li>
<li><strong>Clay oven — alarm interval:</strong> 5 s</li>
<li><strong>Cooking pot — repeat alarm:</strong> Off</li>
<li><strong>Cooking pot — show icon when done:</strong> Off</li>
<li><strong>Cooking pot — alarm interval:</strong> 5 s</li>
<li><strong>Crucible — repeat alarm:</strong> On</li>
<li><strong>Crucible — show icon when done:</strong> On</li>
<li><strong>Crucible — alarm interval:</strong> 5 s</li>
</ul>

<h3>Credits</h3>
<ul>
<li><strong>Code</strong> developed with <a href="https://cursor.com">Cursor</a></li>
<li><strong>Images</strong> generated with <a href="https://gemini.google.com/">Gemini</a></li>
<li><strong>Sounds</strong> adapted from a sample on <a href="https://freesound.org/">FreeSound</a></li>
</ul>
```

---

### Tags

QoL, HUD, UI, Food, Oven, Firepit, Sound, Client, Code — opcional: Utility, Tweak, Crafting

---

### Imagens


| Uso                       | Tamanho            | Onde                                                           |
| ------------------------- | ------------------ | -------------------------------------------------------------- |
| **modicon.png** (in-game) | 256×256            | Raiz do zip                                                    |
| **ModDB Logo**            | 480×480 ou 480×320 | Upload em **Screenshots** → selecionar em **ModDB Logo image** |
| **External Logo**         | —                  | Deixar *Default (crop ModDB image)* se o logo for 480×480      |


Arquivo pronto para o Mod DB: `modicon-resize480x480.png` (redimensionado de `modicon.png`).

**Card Preview** e **External Preview** atualizam sozinhos a partir de **Name**, **Summary** e logo selecionado.

---

### Release

Zip `Unforgettable-mv_1.0.0-gv_1.22.2.zip`:

```
modinfo.json
modicon.png
Unforgettable.dll
assets/unforgettable/...
```

- **Mod version:** 1.0.0  
- **Game version:** 1.22.2

**Changelog:**

```
Initial release — clay oven, firepit cooking pot, and crucible HUD timers with repeating alarms.
```

---

### modinfo.json

```json
"description": "Client-side HUD timers and alarms for clay oven, cooking pot, and crucible. Optional in-game settings via ConfigLib, AutoConfigLib, and ImGui.\n\nDefault settings:\n\nSetting | Default\nLeft margin | 2%\nTop margin | 2%\nIcon size | 80 px\nGap between icons | 10 px\nGap between stations | 10 px\nClay oven — repeat alarm | On\nClay oven — show icon when done | On\nClay oven — alarm interval | 5 s\nCooking pot — repeat alarm | Off\nCooking pot — show icon when done | Off\nCooking pot — alarm interval | 5 s\nCrucible — repeat alarm | On\nCrucible — show icon when done | On\nCrucible — alarm interval | 5 s",
"website": "https://mods.vintagestory.at/show/mod/50588",
"iconPath": "modicon.png"
```

---

### Publicar

1. Preencher **Summary** e **Text** (HTML, Source code do TinyMCE)
2. Tags, logo 480×480 em Screenshots, selecionar **ModDB Logo**
3. Upload do release 1.0.0 / game 1.22.2
4. Status **Draft** → **Published**

