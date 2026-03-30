# Orthogonal Prison Yard Problem Solver

## 🎓 Project Context
This project is a practical implementation developed as part of the **Computational Geometry** course during my 3rd year at the Faculty of Cybernetics. It focuses on solving the visibility and guard placement problem within restricted geometric constraints.

## 🏛️ The Problem: Orthogonal Prison Yard
The **Orthogonal Prison Yard Problem** is a specialized variation of the classic *Art Gallery Problem*. In this scenario, we deal with an **orthogonal polygon** — a shape where every edge meets at a $90^\circ$ or $270^\circ$ angle.

**The Goal:** Determine the minimum number of security cameras (guards) and their optimal positions at the vertices to ensure 100% visibility of the entire yard.

**Mathematical Basis:** According to the Orthogonal Art Gallery Theorem, for any orthogonal polygon with $n$ vertices, $\lfloor n/4 \rfloor$ guards are always sufficient and sometimes necessary.

## 🚀 Features
* **Interactive UI:** Define the yard by clicking to place vertices or drawing edges manually.
* **Optimal Placement:** Calculates the minimum guard count using triangulation and graph coloring techniques.
* **Real-time Visualization:** * Dynamic drawing of the orthogonal boundary.
    * Visual representation of the triangulation/quadrilateralization process.
    * Colored visibility zones for each placed camera.
* **Efficiency:** Designed with $O(n)$ time complexity to handle complex polygons efficiently.

## 🛠️ Technical Stack
* **Language:** C#
* **Framework:** .NET 9 / WPF (Windows Presentation Foundation)
* **Key Algorithms:** Ear Clipping (Triangulation), 3-Coloring, Polygon Visibility.
