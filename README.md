# Unforgettable

Mod cliente para Vintage Story 1.22+ que exibe um ícone animado na HUD e toca um alarme sonoro quando **pão** ou **torta** ficam perfeitamente assados no **forno de barro**, antes de queimarem.

## Como funciona

- Um patch via Harmony intercepta as atualizações do forno de barro a cada ~500ms no lado cliente.
- Enquanto o item está assando, um ícone aparece na lateral esquerda da tela e pisca com frequência crescente (de 0x/s até 2x/s) conforme o preparo avança.
- Quando o item atinge o ponto perfeito (`-perfect`), o alarme toca imediatamente e continua tocando a cada 5 segundos.
- Quando o item é removido do forno, o ícone desaparece e o alarme para.

## Itens monitorados


| Item                     | Estágio anterior    | Estágio detectado   |
| ------------------------ | ------------------- | ------------------- |
| Pão (qualquer grão)      | `bread-*-partbaked` | `bread-*-perfect` ✅ |
| Torta (qualquer recheio) | `pie-partbaked`     | `pie-perfect` ✅     |


## Estrutura do projeto

```
Unforgettable/
├── Unforgettable.csproj
├── modinfo.json
├── Core.cs              — ponto de entrada do ModSystem
├── AlarmSystem.cs       — lógica de estado, progresso e alarme
├── HudRenderer.cs       — renderização do ícone na HUD
├── BakingPatch.cs       — patch Harmony em BlockEntityOven
└── assets/
    └── unforgettable/
        ├── sounds/
        │   └── oventialarm.ogg
        └── textures/
            └── oventimer.png
```

## Instalação

1. Compile com `dotnet build`.
2. Crie um zip com: `modinfo.json`, `Unforgettable.dll` e a pasta `assets/`.
3. Coloque o zip em `%appdata%\VintagestoryData\Mods\`.

## Créditos

- **Código:** desenvolvido com [Cursor Composable](https://cursor.com)
- **Imagens** (ícones HUD e `modicon.png`): geradas com Gemini Nano Banana
- **Som do forno/panela** (`oventialarm.ogg`): adaptado de [484718__ccr_fs__oven-timer-complete](https://freesound.org/) (FreeSound)

Para substituir o som do forno, converta qualquer MP3 para OGG e renomeie para `oventialarm.ogg`.

## Empacotamento

```powershell
dotnet build
$modinfo    = Get-Content bin\modinfo.json | ConvertFrom-Json
$modVersion = $modinfo.version
$gameVersion = $modinfo.dependencies.game
$zipName    = "Unforgettable-mv_$modVersion-gv_$gameVersion.zip"

$out = "bin\Unforgettable-pkg"
New-Item -ItemType Directory -Force $out | Out-Null
Copy-Item bin\modinfo.json $out
Copy-Item bin\Unforgettable.dll $out
Copy-Item -Recurse bin\assets $out
Compress-Archive -Path "$out\*" -DestinationPath $zipName -Force

Copy-Item $zipName "$env:APPDATA\VintagestoryData\Mods\$zipName" -Force
```

