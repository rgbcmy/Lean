# Lean WebUI v1.0.0 - First Official Release 🎉

We are excited to announce the **first official release** of **Lean WebUI** - a modern, full-featured web interface for the QuantConnect Lean algorithmic trading engine!

## 🌟 Highlights

Lean WebUI brings a powerful, user-friendly web interface to Lean, making algorithmic trading accessible to everyone. This release includes:

- 🏦 **IBKR Integration**: Full Interactive Brokers integration for paper and live trading
- 📈 **US Stock Trading**: Real-time trading of US stocks and ETFs
- 🤖 **Strategy Management**: Create, configure, and monitor Lean strategies through the UI
- 📊 **Advanced Charting**: K-line charts, technical indicators, and real-time market data
- ⚡ **Real-Time Updates**: SignalR-powered live order, position, and market data updates
- 🔒 **Enterprise Security**: JWT authentication, audit logging, encrypted credentials
- 🐳 **Docker Ready**: Production-ready Docker deployment
- 🌐 **Multi-Language**: Chinese UI with English documentation

## 🎯 What's Included

### Core Features
- ✅ **IBKR Connection**: Seamless connection to Interactive Brokers TWS/Gateway
- ✅ **Order Management**: Market and limit orders with real-time status updates
- ✅ **Portfolio Tracking**: Live position tracking with P&L calculations
- ✅ **Strategy Execution**: Start, stop, and monitor Lean strategies
- ✅ **Backtesting**: Full backtesting system with parameter optimization
- ✅ **Risk Control**: Stop loss, position limits, and risk metrics
- ✅ **Market Data**: Real-time Level 1 quotes for US stocks and ETFs

### Technical Stack
- **Backend**: ASP.NET Core 10, Entity Framework Core, SignalR
- **Frontend**: React 19, TypeScript, Ant Design
- **Database**: PostgreSQL 16 (production) or SQLite (development)
- **Infrastructure**: Docker, Docker Compose

### Documentation
📖 Complete documentation included:
- User Installation Guide (Chinese & English)
- Developer Guide (Chinese & English)
- API Reference
- IBKR Setup Tutorial
- Docker Deployment Guide
- Operations Manual

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI

# Start services
docker-compose up -d

# Access UI at http://localhost:5000
# Default credentials: admin / Admin@123
```

### Manual Installation

**Prerequisites**: .NET 10 SDK, Node.js 20+, PostgreSQL 16 (optional)

```bash
# Build frontend
cd WebUI/WebUI.Frontend
npm install
npm run build

# Build backend
cd ../WebUI.API
dotnet build

# Run migrations
dotnet ef database update

# Start application
dotnet run
```

See [User Installation Guide](./docs/WebUI用户指南.md) for detailed instructions.

## 📦 Downloads

- **Source Code**: Available on GitHub
- **Docker Image**: `quantconnect/lean-webui:1.0.0` (coming soon to Docker Hub)
- **Documentation**: Included in the `/WebUI/docs` directory

## 🔐 Security Notes

- Default admin password: `Admin@123` - **CHANGE THIS IMMEDIATELY**
- Generate strong JWT secrets for production
- Use HTTPS in production (see [HTTPS Setup Guide](./HTTPS_SETUP.md))
- Review [Security Configuration](./SECURITY_CONFIGURATION.md) before deployment

## 📋 System Requirements

### Minimum
- .NET 10 Runtime
- 2GB RAM
- 10GB disk space
- Windows 10+, Linux (Ubuntu 20.04+), or macOS 12+

### Recommended
- .NET 10 Runtime
- PostgreSQL 16
- 4GB RAM
- 20GB disk space
- Redis (for caching)

## 🐛 Known Issues

None at this time. Please report any issues on our [GitHub Issues](https://github.com/QuantConnect/Lean/issues) page.

## 🎯 Roadmap for v1.1.0

Based on community feedback, we plan to add:
- Multi-user support with RBAC
- Additional broker integrations
- Options and futures trading
- Mobile native apps
- Advanced charting tools
- Strategy marketplace

## 🙏 Acknowledgments

- QuantConnect team for the amazing Lean engine
- Interactive Brokers for their trading API
- All open-source contributors

## 📞 Support & Feedback

- **GitHub Issues**: [Report bugs or request features](https://github.com/QuantConnect/Lean/issues)
- **Documentation**: [WebUI Documentation](./docs/)
- **Discord**: Join our community (link coming soon)

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../LICENSE) file for details.

---

**Full Changelog**: See [CHANGELOG.md](./CHANGELOG.md)

**Installation Guide**: [中文](./docs/WebUI用户指南.md) | [English](./docs/WebUI-User-Guide.md)

**Developer Guide**: [中文](./docs/WebUI开发者完整指南.md) | [English](./docs/WebUI-Developer-Guide.md)

---

Thank you for using Lean WebUI! Happy trading! 🚀📈
