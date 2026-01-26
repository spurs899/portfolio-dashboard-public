# PortfolioDashboard

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

PortfolioDashboard is a .NET 9.0 solution for managing and visualising investment portfolios across multiple brokerages. It provides a modern web interface for viewing aggregated portfolio data from Sharesies and Interactive Brokers.

**🚀 [Live Demo](https://spurs899.github.io/portfolio-dashboard/)** - View the demo with sample portfolio data

## Screenshots

### Dashboard
![Dashboard](Screenshots/dashboard.png)
*Portfolio overview with real-time analytics, holdings breakdown, and multi-brokerage support*

### Mobile View
<img src="Screenshots/mobile.png" alt="Mobile Dashboard" width="375" />

*Responsive mobile layout with condensed holdings list and daily return summary*

### Login Flow
<table>
  <tr>
    <td><img src="Screenshots/login.png" alt="Login Page" /><br/><em>Initial login page</em></td>
    <td><img src="Screenshots/login_credentials.png" alt="Login Credentials" /><br/><em>Enter your credentials</em></td>
    <td><img src="Screenshots/login_mfa.png" alt="MFA Verification" /><br/><em>MFA code verification</em></td>
  </tr>
</table>

## Features

- **Multi-Brokerage Portfolio Aggregation** - View holdings from multiple brokers in one dashboard
- **Real-Time Analytics** - Track total value, daily returns, and performance metrics
- **NYSE Market Status** - Live market hours tracking with holiday calendar support
- **Secure Authentication** - MFA support for Sharesies login
- **Holdings Management** - Detailed instrument breakdown with returns tracking
- **Modern UI** - Blazor WebAssembly with MudBlazor Material Design components
- **Demo Mode** - Try the dashboard with sample data without credentials

## Projects (Public)

This repository showcases selected projects from the complete solution:

- **PortfolioManager.Web**: Blazor WebAssembly frontend with portfolio dashboard and analytics
- **PortfolioManager.Contracts**: Shared DTOs and contracts for API/Core communication

> **Note**: Additional projects (API, Core business logic, and tests) are maintained in a private repository for security and personal reasons.

## Supported Brokerages

### Sharesies ✅ Fully Supported
- Email/password login with MFA support
- Portfolio data retrieval
- Instrument details and pricing
- Real-time holdings tracking

### Interactive Brokers (IBKR) ✅ Fully Supported
- QR code authentication (Playwright automation)
- Portfolio data retrieval
- Account and position data
- Real-time holdings tracking

## Technology Stack

- **Frontend**: Blazor WebAssembly 9.0
- **UI Framework**: MudBlazor 8.15.0
- **Styling**: Sass/SCSS with DartSassBuilder
- **Design System**: Stitch Design System (Tailwind-inspired)
- **API Documentation**: Swagger/OpenAPI
- **Error Tracking**: Sentry

## Styling Architecture

The application uses **Sass (SCSS)** for all styling with automatic compilation via DartSassBuilder.

**Structure:**
- **Component-scoped styles**: Each Blazor component has its own `.razor.scss` file that compiles to `.razor.css`
  - `TimeAndMarketStatusCard.razor.scss` - Market status chip styling
  - Uses `::deep` selector to style MudBlazor child components
- **Global stylesheet**: `wwwroot/css/app.scss` - Main stylesheet with Sass variables, mixins, and responsive design
  - 100+ Sass variables for colors, spacing, typography, transitions, shadows
  - 15+ mixins for common patterns (flexbox, cards, badges, gradients, breakpoints)
  - Compiles to `wwwroot/css/app.css` automatically on build

**Code-behind pattern**: All components use the `.razor.cs` code-behind pattern for clean separation of markup and logic.

## License
This project is licensed under the MIT License.
