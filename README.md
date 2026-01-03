# Core Match 💎

> **"Swipe. Match. Pop."**

Core Match is a grid-based **Match-3 puzzle game** developed with Unity & C#. It features classic swap mechanics, dynamic boosters, and a custom-built animation system optimized for WebGL.

This project was developed as part of the **BIM427 Game Programming** course curriculum at Istanbul Sabahattin Zaim University.

---

## 🎮 Play Online
You can play the WebGL version directly in your browser:
👉 **[Play Core Match on Itch.io](https://yusufdenizdd.itch.io/core-match)**

---

## ⚙️ Technical Highlights
Unlike standard prototypes relying on heavy external libraries, Core Match focuses on **optimization** and **custom algorithmic implementation**:

* **Custom Animation System:** Tile movements and swap mechanics are implemented using **C# Coroutines** and **Vector3.Lerp** for precise control and performance, avoiding the overhead of external tweening libraries (like DOTween).
* **Object Pooling Pattern:** Implemented a pooling system for tiles and particle effects to minimize Garbage Collection (GC) spikes and ensure a smooth 60 FPS on mobile/web.
* **Grid Logic & Algorithms:** Custom grid management system handling match detection, board refill, and deadlock prevention.
* **Booster Logic:** Algorithmic detection for 4-match (Rocket) and 5-match (Bomb) special combinations.

---

## ✨ Key Features
* **Classic Match-3 Gameplay:** Intuitive swap and match mechanics.
* **Dynamic Boosters:** Create powerful explosives with smart moves.
* **Responsive Design:** Optimized UI that adapts to different screen aspect ratios.
* **Visual Feedback:** "Juicy" interactions with particle effects and screen shake.

---

## 🛠️ Tech Stack
* **Engine:** Unity 2022.3 (LTS)
* **Language:** C#
* **Platform:** WebGL / PC (Windows & macOS)
* **Version Control:** Git

---

## 📜 License
Developed for educational purposes within the scope of IZU - BIM427 Game Programming Course.
