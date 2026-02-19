# 📦 Bulky Project (Udemy 5th Project)

An **ASP.NET Core MVC (Razor Views)** application built to practice real-world MVC patterns and commonly used web features in production systems.

> ⚠️ This project uses **Razor Views (MVC)** — **not Blazor Server** and **not WebAssembly**.

🎓 **Udemy Course:** https://www.udemy.com/course/complete-aspnet-core-21-course/  
📜 **Certificate:** https://drive.google.com/file/d/1AvNL8t0cKJdo7MDJPV2DlrZAxFdVpXzJ/view?usp=drive_link  
🎥 **Project Walkthrough Video:** https://drive.google.com/file/d/1ep8fsAx7u8TQtSADNG51c28q0GP611kr/view?usp=drive_link  

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
