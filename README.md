Udemy 5 th Prject

# 📦 Bulky Project

An **ASP.NET Core MVC (Razor Views)** application built to practice real-world MVC patterns and commonly used web features in production systems.

> ⚠️ This project uses **Razor Views (MVC)** — **not Blazor Server** and **not WebAssembly**.

---

## 🏗 Architecture

- **3-Layer Architecture**
  - Presentation (ASP.NET Core MVC – Razor Views)
  - Business Logic
  - Data Access
- **4th Utility Layer**
  - Helpers
  - Constants
  - Shared services

---

## 🎨 UI & Frontend

- **Bootswatch** for theming
- **Toastr + TempData** for notifications
- **SweetAlert** for confirmation dialogs
- **TinyMCE (Tiny Cloud)** rich text editor for textareas
- **DataTables** (client-side):
  - Search
  - Pagination
  - Ordering  
  > All data is loaded once, then filtered and paginated on the frontend

---

## 🧭 MVC Concepts

- **Areas** for modular feature separation
- **Upsert Pattern** (Create & Update using a single action)
- **View Components** for reusable UI blocks
- Access query parameters using:
  ```csharp
  Context.Request.Query
