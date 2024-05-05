# 1 Project Details
This Unity game project is build using the OOP paradigm where I adhere to the 4 pillars of good OOP design: **Abstraction**, **Polymorphism**, **Encapsulation** and **Inheritance**. The project is intended as an application of important (software) design principles such as **Encapsulate What Varies** and  **Favor Composition Over Inheritance**. Over time, I intend to become a more experienced OOP programmer by applying more and more principles and patterns. This small project serves as a humble beginning.
# 2 What I've learned
My newfound insights fall into two categories: OOP programming and Unity specific.
## 2.1 - Unity Specific 
- Only expose a reference if it does **not** originate from the parent game object. If it does, simply keep the reference private and call `GetComponent<>` in `Start()` or `Awake()`. This minimizes the chance of human mistakes.
