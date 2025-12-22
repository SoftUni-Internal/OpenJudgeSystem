# Online Judge System - Complete Onboarding Plan

## Overview
This is a comprehensive 4-week onboarding plan to master the Online Judge System. You'll be supporting this project alone after a month, so this plan covers business logic, technologies, operations, and troubleshooting.

---

## **Phase 1: Foundation & Architecture (Week 1)**

### **Day 1-2: High-Level Understanding**

#### Day 1: Business Domain
- [-] Read /mnt/data/PRD.txt (Product Requirements)
- [-] Understand: This is an Online Judge System (like LeetCode/HackerRank)
- [-] Core flow: Students submit code → System tests it → Provides feedback
- [ ] Key entities: Contests, Problems, Submissions, Users, Test Cases

#### Day 2: Architecture Overview
- [ ] Study docker-compose.yml (service relationships)
- [ ] Map out: UI → API → Worker → Database flow
- [ ] Understand: Message queue pattern (RabbitMQ)
- [ ] Identify: 3 main applications (UI, Administration, Worker)

### **Day 3-4: Technology Stack Deep Dive**

#### Backend (.NET 8)
- [ ] Study: Servers/UI/OJS.Servers.Ui/Program.cs (service registration)
- [ ] Understand: Dependency injection pattern
- [ ] Learn: MassTransit (message queue library)
- [ ] Review: Entity Framework + Prisma patterns

#### Frontend (React + Vite)
- [ ] Study: Servers/UI/OJS.Servers.Ui/ClientApp/src/redux/store.ts
- [ ] Understand: Redux Toolkit Query pattern
- [ ] Review: TypeScript interfaces in src/common/types/
- [ ] Learn: Multi-page application setup (vite.config.js)

#### Infrastructure
- [ ] Docker: Multi-stage builds, base images
- [ ] RabbitMQ: Message patterns, pub/sub
- [ ] Databases: SQL Server (main), MySQL, PostgreSQL
- [ ] Redis: Session management, caching

### **Day 5-7: Core Business Logic**

#### Submission Flow (CRITICAL)
- [ ] Study: Services/UI/OJS.Services.Ui.Business/Implementations/SubmissionsBusinessService.cs
- [ ] Trace: Submit() method → PublishSubmissionForProcessing()
- [ ] Understand: How code gets from UI to Worker
- [ ] Debug: Follow a submission end-to-end

#### Worker Execution (MOST COMPLEX)
- [ ] Study: Servers/Worker/OJS.Servers.Worker/Consumers/SubmissionsForProcessingConsumer.cs
- [ ] Understand: How worker receives jobs
- [ ] Review: Services/Common/OJS.Workers/ (execution strategies)
- [ ] Learn: Docker-in-Docker execution pattern

#### Contest Management
- [ ] Study: Controllers/CompeteController.cs
- [ ] Understand: Contest participation flow
- [ ] Review: Time limits, scoring, rankings

---

## **Phase 2: Hands-On Development (Week 2)**

### **Day 8-10: Local Development Setup**

#### Day 8: Environment Setup
- [ ] Clone repository
- [ ] Run: docker-compose up (full stack)
- [ ] Run: Frontend separately (yarn start)
- [ ] Verify: All services healthy
- [ ] Test: Submit a simple "Hello World" program

#### Day 9: Database Exploration
- [ ] Connect to SQL Server (port 1433)
- [ ] Study: Database schema (tables, relationships)
- [ ] Review: Data/OJS.Data/Models/ (Entity Framework models)
- [ ] Understand: Submissions, Contests, Problems tables

#### Day 10: API Testing
- [ ] Use Swagger UI (http://localhost:5010/swagger)
- [ ] Test: All major endpoints
- [ ] Study: Request/Response models
- [ ] Debug: API calls with browser dev tools

### **Day 11-14: Code Execution Deep Dive**

#### Day 11: Execution Strategies
- [ ] Study: Services/Common/OJS.Workers/OJS.Workers.ExecutionStrategies/
- [ ] Pick one: JavaSpringAndHibernateProjectExecutionStrategy.cs
- [ ] Understand: How user code is compiled and tested
- [ ] Learn: Security model (restricted_user)

#### Day 12: Worker Debugging
- [ ] Set breakpoints in: SubmissionsForProcessingConsumer.cs
- [ ] Submit test code and step through execution
- [ ] Watch: Message queue flow (RabbitMQ management UI)
- [ ] Understand: Error handling and logging

#### Day 13: Language Support
- [ ] Study: Different execution strategies (Java, Python, C#, JavaScript)
- [ ] Test: Submit code in each supported language
- [ ] Understand: How Docker containers are used for isolation
- [ ] Review: Base image contents (judge_worker_base)

#### Day 14: Testing Framework
- [ ] Study: How test cases are structured
- [ ] Review: Input/output validation
- [ ] Understand: Scoring algorithms
- [ ] Test: Create a simple problem with test cases

---

## **Phase 3: Advanced Features (Week 3)**

### **Day 15-17: Advanced Business Logic**

#### Day 15: Contest Management
- [ ] Study: Contest creation and management
- [ ] Understand: Official vs Practice modes
- [ ] Review: Time limits and deadlines
- [ ] Test: Create and participate in a contest

#### Day 16: User Management & Security
- [ ] Study: Authentication/authorization
- [ ] Review: Role-based access (Admin, Lecturer, Student)
- [ ] Understand: Session management (Redis)
- [ ] Test: Different user roles and permissions

#### Day 17: Mentor System (AI Integration)
- [ ] Study: Services/Mentor/OJS.Services.Mentor.Business/
- [ ] Understand: OpenAI integration
- [ ] Review: How AI provides coding help
- [ ] Test: Mentor conversations

### **Day 18-21: Monitoring & Operations**

#### Day 18: Logging & Telemetry
- [ ] Study: Services/Common/OJS.Services.Common/Telemetry/
- [ ] Understand: OpenTelemetry integration
- [ ] Review: Structured logging patterns
- [ ] Learn: How to trace requests across services

#### Day 19: Background Jobs
- [ ] Study: Hangfire integration
- [ ] Review: Services/Administration/OJS.Services.Administration.Business/Implementations/RecurringBackgroundJobsBusinessService.cs
- [ ] Understand: Cleanup jobs, health checks
- [ ] Test: Trigger background jobs manually

#### Day 20: Performance & Scaling