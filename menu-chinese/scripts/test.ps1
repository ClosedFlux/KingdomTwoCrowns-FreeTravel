param([string]$GameDirectory)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Add-Type -Path (Join-Path $root 'src_plugin\MenuChinese.cs'),(Join-Path $root 'src_plugin\MenuTerms.cs')
$checks = @{
    'KingdomMod.OverlayMap' = '地图与统计';
    'Global' = '全局设置';
    'Language' = '语言';
    'GuiUpdatesPerSecond' = '界面每秒刷新次数';
    'Logging.Disk' = '文件日志';
    'Enabled' = '启用';
    'WriteUnityLog' = '写入游戏引擎日志';
    'SaveDataExtras' = '存档附加数据';
    'GuiStyle.zh-CN' = '界面样式（简体中文）';
    'Language.zh-CN' = '地图名称（简体中文）';
    'MarkerStyle' = '地图标记样式';
    'MapOffset' = '地图水平偏移';
    'ZoomScale' = '地图缩放比例';
    'ExploredLeft' = '已探索左边界';
    'ExploredRight' = '已探索右边界';
    'Time' = '时间';
    'Days' = '天数';
    'island-v35-c0-l4' = '战役 1 · 第 5 岛';
    'Castle.Locked' = '城堡 · 未解锁';
    'TopMap.Count' = '地图 · 数量';
    'system' = '跟随系统';
    'zh-CN' = '简体中文';
    'Increase to be more accurate, decrease to reduce performance impact' = '提高刷新次数可提升准确性，降低可减少性能开销。'
}
foreach ($key in $checks.Keys) {
    if ([ConfigManager.MenuChinese]::Text($key) -ne $checks[$key]) { throw "Missing translation: $key" }
}
foreach ($value in @('未知选项','MarkerStyle.cfg','F1','0.1.1','com.sinai.BepInExConfigManager')) {
    if ([ConfigManager.MenuChinese]::Text($value) -ne $value) { throw "Technical value was changed: $value" }
}
if ([ConfigManager.MenuChinese]::Bool($true) -ne '开启' -or [ConfigManager.MenuChinese]::Bool($false) -ne '关闭') { throw 'Boolean translation failed' }
Write-Output 'PASS: Chinese menu labels, descriptions, boolean captions and unchanged technical values.'
$configFiles = @()
if ($GameDirectory) { $configFiles = @(Get-ChildItem (Join-Path $GameDirectory 'BepInEx\config') -Recurse -Filter '*.cfg') }
$missing = @()
foreach($file in $configFiles) {
    foreach($line in Get-Content $file.FullName) {
        $token = if($line -match '^\[(.+)\]$') { $Matches[1] } elseif($line -match '^([^#\s][^=]+) = ') { $Matches[1].Trim() } else { continue }
        if($token -match '[A-Za-z]' -and [ConfigManager.MenuChinese]::Text($token) -eq $token) { $missing += $token }
    }
}
if($missing.Count -gt 0) { throw ('Untranslated config labels: '+(($missing | Sort-Object -Unique) -join ', ')) }
if ($configFiles.Count) { Write-Output 'PASS: installed configuration labels covered.' }
if ([ConfigManager.MenuChinese]::Option('Fatal, Error, Warning') -ne '致命错误、错误、警告') { throw 'Log levels not translated.' }
if ([ConfigManager.MenuChinese]::ColorChannel('A') -ne '透明度') { throw 'Color channels not translated.' }
if ([ConfigManager.MenuChinese]::SettingType([float]) -ne '小数') { throw 'Setting type not translated.' }
if ([ConfigManager.MenuChinese]::Description('Preloader','LoadDumpedAssemblies','English description') -notmatch '调试器') { throw 'Missing loader description.' }
$untranslatedDescriptions = @()
foreach($file in $configFiles) {
    $description = @()
    $section = ''
    foreach($line in Get-Content $file.FullName) {
        if($line -match '^\[(.+)\]$') { $section=$Matches[1]; $description=@() }
        elseif($line -match '^## (.*)') { $description += $Matches[1] }
        elseif($line -match '^([^#\s][^=]+) = ') {
            $key=$Matches[1].Trim()
            $original=$description -join "`n"
            $translated=[ConfigManager.MenuChinese]::Description($section,$key,$original)
            if($original -match '[A-Za-z]{3}' -and $translated -eq $original -and $original -notmatch '[\p{IsCJKUnifiedIdeographs}]') { $untranslatedDescriptions += "$section/$key" }
            $description=@()
        }
        elseif($line.Trim() -eq '') { $description=@() }
    }
}
if($untranslatedDescriptions.Count) { throw ('Untranslated descriptions: '+($untranslatedDescriptions -join ', ')) }
Write-Output 'PASS: description translations, log options, color channels and setting types checked.'
