# Light Probe Box

Makes placing and editing Unity's Light Probe Groups fast and easy.

Arranges the light probes evenly inside the defined bounds (a 'Light Probe Box') while respecting scene geometry.

<img width="1280" height="640" alt="example-image" src="https://github.com/user-attachments/assets/d637f9c0-7209-4a07-9b9e-ab7a38c6a253" />

[![Unity Version](https://img.shields.io/badge/unity-2019.1+-blue)]()
[![Unity Version](https://img.shields.io/badge/unity-6-blue)]()
[![License: MIT](https://img.shields.io/badge/license-MIT-green)]()


---

## Key features
- Creates and arranges light probes for you
- Prevents clipping with geometry
- Prevents redundant overlapping placements with other Light Probe Boxes
- Supports simple and body-centered cubic lattices
- Supports adjusting individual light probes by hand

---

## How to install
1. Navigate to the Package Manager inside Unity
2. Click the '+' icon, and then the '_Install package from git URL..._' button
3. Insert 'https://github.com/NVK-fi/LightProbeBox.git'
4. Press _Install_

---

## How to use
Add a new Light Probe Box:
1. Right click Hierarchy panel, or press '_Add Component_' on the Inspector panel
2. Choose _Light_ -> _Light Probe Box_

Once added:
- Resize the bounds by selecting and dragging the orange handles
- Adjust the lattice properties and collision resolver settings
- Select all of the Light Probe Boxes which need to be (re)generated
- Press _Regenerate Probes_ to see the results

https://github.com/user-attachments/assets/a39fdfe4-547f-41f3-a845-247532cbf9ce

