# SRM-SOC: Student Risk Management System

![Status](https://img.shields.io/badge/Status-Development-orange)
![Category](https://img.shields.io/badge/Category-Cyber_Security-blue)

**SRM-SOC** is a specialized student tracking and risk analysis engine inspired by **Security Operations Center (SOC)** principles. It treats student attendance data as "network telemetry" and applies automated rules to detect anomalies and trigger incident responses.

## 🛡️ SOC Concepts Applied
This project isn't just an attendance app; it's a simulation of a security monitoring environment:
- **Log Source:** Student attendance inputs are treated as logs.
- **Detection Rule:** An automated trigger activates when a student hits 4 absences (Threshold: 80% Risk).
- **Incident Response (SOAR):** Once the risk score threshold is met, the system automatically triggers an email notification via **MailKit** to initiate a response.

## 🚀 Features
- **Real-time Risk Scoring:** Dynamic calculation based on attendance logs.
- **Automated Alerts:** Instant email notifications for high-risk students.
- **Bulk Data Handling:** Engineered for high-performance data entry.
- **Clean Architecture:** Built on ASP.NET Core for scalability and security.

## 🛠️ Tech Stack
- **Backend:** ASP.NET Core
- **Mailing:** MailKit & MimeKit
- **Database:** SQL Server
- **Architecture:** Repository Pattern

## 🔒 Security Best Practices (Planned/Implemented)
- **Input Validation:** Protection against common injection attacks.
- **Parameterized Queries:** Ensuring data integrity and preventing SQLi.
- **Secure Configuration:** Sensitive credentials are kept outside the codebase.

## 📧 Contact
**Kerem Korkut Turan** *Computer Engineering Student & Aspiring SOC Analyst* [GitHub Profile](https://github.com/keremtrn-cloud)
