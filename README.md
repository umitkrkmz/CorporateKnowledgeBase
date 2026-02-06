# Corporate Knowledge Base

A full-featured, enterprise-grade knowledge management platform built with **ASP.NET Core 10.0** and **Clean Architecture**. Designed for organizations to centralize documents, blog posts, announcements, and leverage AI-powered search across their knowledge base.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## Features

### Core
- **Document Management** - Create, edit, version, and organize documents with categories and tags
- **Blog Posts** - Rich content publishing with likes, comments, and content moderation
- **Announcements** - Organization-wide announcements with admin controls
- **Full-Text Search** - Search across all content types with filtering

### AI & Intelligence
- **RAG AI Assistant** - Ollama-powered chat assistant that answers questions using your knowledge base
- **Semantic Search** - AI-enhanced document retrieval and recommendations

### Collaboration
- **Real-Time Notifications** - SignalR-powered instant notifications
- **Comments & Likes** - Engage with content through comments and reactions
- **Content Reporting** - Flag inappropriate content for admin review

### Administration
- **Role-Based Access Control** - Admin, User roles with granular permissions
- **Content Review Workflow** - Approve/reject content before publication
- **User Management** - Manage users, roles, and departments
- **Audit Logging** - Track all significant actions for compliance
- **Background Jobs** - Hangfire-powered scheduled tasks and maintenance

### Design
- **Custom Design System** - Consistent, professional UI with `kb-*` CSS class system
- **Sliding Auth Page** - Login/Register with animated sliding cover panel
- **Owl Mascot** - Fun password field mascot that covers eyes during typing
- **Responsive** - Fully responsive across desktop, tablet, and mobile
- **SVG Icons** - Lightweight inline SVG icons throughout

---

## Architecture

```
CorporateKnowledgeBase/
|-- CorporateKnowledgeBase.Domain/          # Entities, Enums, Interfaces
|-- CorporateKnowledgeBase.Application/     # CQRS Commands/Queries, DTOs, MediatR
|-- CorporateKnowledgeBase.Infrastructure/  # EF Core, Identity, Services, Ollama
|-- CorporateKnowledgeBase.Web/             # ASP.NET Core MVC, Views, Controllers
```

**Clean Architecture** with 4 layers:
- **Domain** - Core business entities and interfaces (zero dependencies)
- **Application** - Business logic with MediatR CQRS pattern
- **Infrastructure** - Data access (EF Core + SQL Server), Identity, external services
- **Web** - Presentation layer with Razor Views and Controllers

---

## Tech Stack

| Category | Technology |
|----------|-----------|
| **Framework** | ASP.NET Core 10.0 MVC |
| **ORM** | Entity Framework Core 10.0 |
| **Database** | SQL Server |
| **Authentication** | ASP.NET Core Identity |
| **Mediator** | MediatR (CQRS) |
| **Real-Time** | SignalR |
| **Background Jobs** | Hangfire |
| **AI/LLM** | Ollama + Llama 3.2 (RAG) |
| **PDF Generation** | QuestPDF |
| **CSS Framework** | Bootstrap 5 + Custom Design System |
| **Tag Input** | Tagify |

---

## Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full)
- [Ollama](https://ollama.ai/) (optional, for AI features)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/umitkrkmz/CorporateKnowledgeBase.git
   cd CorporateKnowledgeBase
   ```

2. **Configure the database**

   Update `appsettings.json` in the Web project with your connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CorporateKnowledgeBase;Trusted_Connection=True;"
     }
   }
   ```

3. **Apply migrations**
   ```bash
   cd CorporateKnowledgeBase.Web
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open in browser**

   Navigate to `https://localhost:5001` (or the port shown in console)

### Default Admin Account

On first run, the system automatically creates:
- **Email:** `admin@corp.com` | **Password:** `Admin123!` | **Role:** Admin

To load demo data (users, documents, blog posts, etc.), go to **Admin → Settings → Demo Verileri Yükle**.

### AI Setup (Optional)

1. Install [Ollama](https://ollama.ai/)
2. Pull the required models:
   ```bash
   ollama pull llama3.2           # Chat model (3B, recommended)
   ollama pull nomic-embed-text   # Embedding model (required for RAG)
   ```
3. Ensure Ollama is running on `http://localhost:11434`
4. The AI Assistant will automatically connect

#### Alternative AI Models

The default model is **Llama 3.2 3B** — a good balance of quality and performance. You can switch models in `Program.cs`:

| Model | Size | VRAM | Best For |
|-------|------|------|----------|
| `llama3.2` (default) | ~2 GB | 3-4 GB | General use, good Turkish support |
| `llama3.2:1b` | ~700 MB | 1-2 GB | Raspberry Pi, low-resource devices |
| `phi3` | ~2.3 GB | 3-4 GB | Code-heavy knowledge bases |
| `mistral` | ~4.1 GB | 5-6 GB | Higher quality responses |
| `gemma2` | ~5 GB | 6-8 GB | Best multilingual support |

To change the model, update `Program.cs`:
```csharp
builder.Services.AddChatClient(new OllamaApiClient(ollamaUri, "your-model-here"));
```

> **Raspberry Pi / Low-resource:** Use `llama3.2:1b` for 5-10 concurrent users. For 10-200 users, a server with at least 16 GB RAM and a dedicated GPU (e.g., RTX 3060) is recommended with `llama3.2` or `mistral`.

---

## Project Structure

### Views
| View | Description |
|------|------------|
| `Account/Login` | Unified login/register with sliding cover + owl mascot |
| `Home/Index` | Guest landing page + authenticated dashboard |
| `Documents/*` | Document CRUD with versioning |
| `BlogPosts/*` | Blog post CRUD with comments & likes |
| `Announcements/*` | Announcement management |
| `AI/Index` | RAG-powered AI chat assistant |
| `Profile/*` | User profile, edit, notification settings |
| `Search/Index` | Full-text search across content |
| `Faq/*` | FAQ management |
| `Notifications/Index` | User notification center |
| `Admin*/*` | Admin panels (Categories, Tags, Users, Departments, Reports) |
| `ContentReview/Index` | Content moderation queue |
| `AuditLogs/Index` | System audit trail |

### CSS Design System
All views use a consistent `kb-*` CSS class system defined in `wwwroot/css/site.css`:

- **Navbar**: `kb-navbar`, `kb-search-form`, `brand-icon`, `brand-text`, `brand-highlight`
- **Layout**: `kb-page-header`, `kb-panel`, `kb-card`, `kb-card-body`
- **Tables**: `kb-table` with styled `thead`/`tbody`
- **Badges**: `kb-badge-primary`, `kb-badge-success`, `kb-badge-warning`, `kb-badge-danger`, `kb-badge-tag`
- **Buttons**: `kb-btn-primary`, `kb-btn-outline`, `kb-btn-success`, `kb-btn-danger`, `kb-btn-sm`
- **Forms**: `kb-form-card`, `kb-form-group`, `kb-form-label`, `kb-form-control`
- **Alerts**: `kb-alert-success`, `kb-alert-warning`, `kb-alert-danger`, `kb-alert-info`
- **Details**: `kb-detail-header`, `kb-detail-meta`, `kb-detail-badges`, `kb-detail-actions`
- **Profile**: `kb-profile-card`, `kb-profile-header`, `kb-profile-avatar`
- **Chat**: `kb-chat-container`, `kb-chat-messages`, `kb-chat-bubble`, `kb-chat-user`, `kb-chat-bot`, `kb-chat-avatar`
- **AI Content**: `kb-ai-content`, `kb-ai-source-badge` for code blocks and source citations
- **Comments**: `kb-comment`, `kb-comment-avatar`, `kb-comment-body`, `kb-like-btn`
- **Landing**: `kb-hero`, `kb-hero-title`, `kb-feature-card`, `kb-stats`, `kb-stat-card`
- **Admin**: `kb-admin-page`, `kb-table-toolbar`, `kb-filter-select`, `kb-sort-link`
- **Pagination**: `kb-pagination` with Bootstrap-compatible styling
- **Empty State**: `kb-empty-state` for no-data views
- **Content**: `content-body` for rendered markdown with code highlighting

---

## Version History

| Version | Architecture | Status |
|---------|-------------|--------|
| **v1.0** (current) | Clean Architecture + MediatR CQRS + .NET 10.0 | Active |
| **v1.0 - v3.0** (legacy) | Monolithic MVC + ASP.NET Core 9.0 | [Archived](https://github.com/umitkrkmz/CorporateKnowledgeBase-legacy) |

> The legacy version (v1.0 - v3.0) was a monolithic MVC application that evolved through three major releases, culminating in RAG AI integration, Hangfire background jobs, and SignalR real-time features. This current repository is a complete rewrite from the ground up with **Clean Architecture**, **Vertical Slice**, and **MediatR (CQRS)** patterns.
>
> Legacy repository: [CorporateKnowledgeBase-legacy](https://github.com/umitkrkmz/CorporateKnowledgeBase-legacy)

### v1.0 Changelog (Clean Architecture Rewrite)

#### 🏗️ Architecture
- **Complete rewrite** from monolithic MVC to 4-layer Clean Architecture
- **Domain Layer** - Pure entities with zero dependencies
- **Application Layer** - MediatR CQRS (Commands/Queries) + Pipeline Behaviors
- **Infrastructure Layer** - EF Core, Identity, Ollama AI services
- **Web Layer** - ASP.NET Core MVC with Razor Views

#### 🎨 Design System
- **Custom `kb-*` CSS framework** (~3000 lines) replacing ad-hoc Bootstrap usage
- **Sliding auth page** with animated cover panel + owl mascot
- **Modern logo** with hexagon knowledge network icon
- **Consistent design tokens** via CSS variables

#### 🤖 AI Improvements  
- Upgraded from **Phi-3** to **Llama 3.2** (better Turkish support)
- **QuestPDF** replaced PuppeteerSharp for lighter PDF generation
- Session-based chat history with archiving

#### ⚡ Technical
- **.NET 10.0** (upgraded from 9.0)
- **Audit logging** via MediatR pipeline behavior
- **Department-based content filtering** for privacy
- **User approval workflow** for new registrations

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
