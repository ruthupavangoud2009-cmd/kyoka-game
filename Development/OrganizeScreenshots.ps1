$src = "D:\bots updated\KYOKA_Full_Unity_Project\KYOKA_Full_Unity_Project\Builds\Windows\Builds\Screenshots"
$dst = "D:\bots updated\KYOKA_Full_Unity_Project\KYOKA_Full_Unity_Project\Builds\Screenshots"

if (-not (Test-Path $dst)) {
    New-Item -ItemType Directory -Path $dst -Force | Out-Null
}

Copy-Item "$src\2_CharacterCustomizer.png" "$dst\1_Character.png" -Force
Copy-Item "$src\3_MainLobby.png" "$dst\2_MainLobby.png" -Force
Copy-Item "$src\4_BattleRoyale.png" "$dst\3_Combat.png" -Force
Copy-Item "$src\5_OpenWorld.png" "$dst\4_City.png" -Force
Copy-Item "$src\5_Forest.png" "$dst\5_Forest.png" -Force
Copy-Item "$src\6_Ocean.png" "$dst\6_Ocean.png" -Force

Get-ChildItem $dst | Select-Object Name, Length, LastWriteTime
