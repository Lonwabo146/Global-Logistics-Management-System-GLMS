Global Logistics Management System (GLMS)
Overview
The Global Logistics Management System (GLMS) is an enterprise-grade application designed to manage end-to-end logistics operations across international supply chains. It provides real-time tracking, route optimisation, shipment scheduling, and multi-stakeholder coordination for global freight and cargo management.

Table of Contents

Features
Architecture
Design Patterns
Non-Functional Requirements
Getting Started
Project Structure
Technologies Used
Academic Context


Features

Shipment creation, tracking, and lifecycle management
Route planning and optimisation across multiple carriers
Real-time status updates and notifications
Multi-currency and multi-language support
Carrier and vendor management
Customs documentation and compliance reporting
Role-based access control for stakeholders (admins, carriers, clients)
Audit logging for all transactions


Architecture
The GLMS is designed using the TOGAF (The Open Group Architecture Framework), structured across the following architecture domains:
DomainDescriptionBusiness ArchitectureLogistics workflows, stakeholder roles, and business rulesData ArchitectureEntity models for shipments, routes, carriers, and clientsApplication ArchitectureService layers, APIs, and UI componentsTechnology ArchitectureCloud infrastructure, databases, and integration points
For detailed framework analysis, refer to the Enterprise Analysis & Architecture Report included in the project documentation.

Design Patterns
The system implements the following Gang of Four (GoF) design patterns:
1. Factory Method (Creational)
Used to instantiate different shipment types (air, sea, road, rail) without coupling the client code to concrete classes.
2. Observer (Behavioural)
Applied to the shipment tracking module, where status updates trigger notifications to subscribed stakeholders in real time.
3. Facade (Structural)
Provides a unified interface to the complex subsystems handling route calculation, carrier APIs, and customs validation, simplifying interaction for the application layer.
UML class diagrams for each pattern are included in the /docs/diagrams directory.

Getting Started
Prerequisites

.NET 8 SDK
SQL Server / Azure SQL Database
Node.js (for any frontend tooling)
Azure subscription (for cloud deployment)

Installation
bash# Clone the repository
git clone https://github.com/your-username/glms.git
cd glms

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run

roject Structure
GLMS/
├── docs/
│   ├── architecture/        # TOGAF framework documentation
│   ├── diagrams/            # UML class diagrams
│   └── reports/             # Enterprise Analysis & Architecture Report
├── src/
│   ├── GLMS.Core/           # Domain models and business logic
│   ├── GLMS.Application/    # Application services and use cases
│   ├── GLMS.Infrastructure/ # Data access, external APIs
│   └── GLMS.Web/            # MVC controllers and views
├── tests/
│   └── GLMS.Tests/          # Unit and integration tests
├── README.md
└── GLMS.sln

Technologies Used

Framework: ASP.NET Core 8 MVC
ORM: Entity Framework Core 8
Database: Azure SQL Database
Cloud: Azure App Service
Version Control: Git / GitHub
CI/CD: GitHub Actions
Architecture Framework: TOGAF
