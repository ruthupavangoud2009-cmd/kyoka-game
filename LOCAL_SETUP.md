# KYOKA — Local Development & LAN Testing Setup

Instructions for testing KYOKA locally on Windows PC and running two-device multiplayer over local Wi-Fi / LAN with zero paid cloud hosting costs.

---

## 1. Local Single-Player Testing on Windows
1. Open the project in Unity 6.
2. Open `Assets/KYOKA/Scenes/KYOKA_Bootstrap.unity` and hit Play.
3. Test controls:
   - `WASD`: Walking / Running. Hold `Shift` to Sprint. `Space` to Jump.
   - `C`: Crouch toggle.
   - `Left Click`: Attack (3-hit melee combo with sword slash trail).
   - `Right Click`: Defend (guard stance, reduces incoming damage by 65%).
   - `Q`: Cast Tempest Dash.
   - `F`: Cast Flame Surge (explosive projectile).
   - `R`: Cast Thunder Strike (lightning pillar).
   - `T`: Activate Kyoka Awakening (aura buff).
   - `E`: Board adjacent vehicle (Car, Motorcycle, Boat, Aircraft).
   - `V`: Switch first-person and third-person camera perspective.
   - `B`: Place creative block in front of player.
   - Click **Save** in top bar to test saving to `Application.persistentDataPath`.

---

## 2. Local Two-Device LAN Multiplayer Testing
KYOKA includes a built-in direct TCP socket networking provider in `KyokaNetworkManager.cs`.

### Step 1: Start Host (PC 1)
1. Run KYOKA on PC 1.
2. Open the console or call `KyokaNetworkManager.Instance.StartLanHost(7777);`.
3. The server starts listening on TCP port 7777.
4. Check PC 1's local IP address using `ipconfig` (e.g. `192.168.1.105`).

### Step 2: Connect Client (PC 2 or Android Phone)
1. Ensure Device 2 is connected to the same Wi-Fi router / local network.
2. Run KYOKA on Device 2.
3. Call `KyokaNetworkManager.Instance.ConnectLanClient("192.168.1.105", 7777);`.
4. Client handshakes with Host. Remote player avatar appears in the world.
5. Transforms, health changes, and combat actions replicate across both screens.
