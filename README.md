# Monrak DesktopServer

[![Platform](https://img.shields.io/badge/platform-Windows-0078d7.svg)](https://www.microsoft.com/windows)
[![PHP](https://img.shields.io/badge/PHP-8.4-777bb4.svg)](https://www.php.net/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.txt)
[![Status](https://img.shields.io/badge/status-Beta-orange.svg)](#)

**Monrak DesktopServer** is a professional, native local development stack for Windows. It provides a streamlined, high-performance environment for modern PHP, MySQL, and PostgreSQL development—designed specifically for developers who prioritize execution speed and system efficiency.

[Features](#-key-features) • [Tech Stack](#-tech-stack) • [Versions](#-version-comparison) • [Getting Started](#-getting-started)

---

## 🧩 Three Editions, One Stack

Developed by **Monrak Net Technology**, DesktopServer focuses on speed, stability, and professional-grade tools for both legacy maintenance and modern web application development. Every edition ships the same .NET 9 management console — pick the engine that matches your work.

### 🪶 Lite — *Essential*
Perfect for lightweight development and low memory usage. Apache 2.4, PHP 7.4 and MySQL 8.0 on a common path, with nothing extra to configure.

### 🧰 Pro/Advance — *Professional*
The ultimate power-user stack with multi-PHP and isolated environments. Switch between PHP 5.6, 7.4, 8.2 and 8.4 in real time, each on its own isolated process path, with VC++ 2012-2022 bundled.

### 🏎️ Go! — *Next-Gen*
Unleash pure performance with RoadRunner and PostgreSQL. Built for AI and modern PHP development — PHP 8.4, MariaDB 11, PostgreSQL 17 with pgvector, a real-time monitoring dashboard and automatic SSL CA trusting.

---

## 🎯 Our Mission: Performance-First Development

We believe that professional development environments should be fast, reliable, and accessible on any hardware.

*   **Modern Standards**: **PHP 8.4** in Pro and Go!, with **PostgreSQL 17** and **RoadRunner** out of the box in Go!.
*   **Native Execution**: Unlike Docker or Virtualization, DesktopServer runs natively on Windows for minimum overhead and maximum speed.
*   **Hardware Optimized**: Designed to be lightweight and memory-efficient, giving older machines a "Pro" development experience.

---

## 🚀 Key Features

*   **Unified Stack**: Pre-configured Apache 2.4 with MySQL 8.0 *(Lite & Pro)*, or RoadRunner with MariaDB 11 *(Go!)*.
*   **Multi-PHP Switching**: PHP 5.6, 7.4, 8.2 and 8.4 installed side by side, switchable in real time *(Pro)*.
*   **Go! Edition Engine**: Integrated **RoadRunner** high-performance PHP orchestrator for sub-5ms response times *(Go!)*.
*   **Database Excellence**: MySQL 8.0 and MariaDB 11, plus **PostgreSQL 17 (AI-Ready)** with pgvector *(Go!)*.
*   **Native Local SSL**: Built-in Automated Certificate Authority (CA) and SSL/HTTPS management with SAN support—get green locks on `localhost` instantly.
*   **Isolated Environments**: Prevents system-wide path conflicts with smart process isolation *(Pro & Go!)*.
*   **Real-time Monitoring**: Integrated dashboard for CPU and RAM performance tracking *(Go!)*.
*   **Premium Management**: Custom .NET 9 management UI with system tray integration and live log viewers.

---

## 🛠️ Tech Stack

Not every component ships in every edition — see the [comparison](#-version-comparison) for what each one includes.

### Core
*   **Management Console**: .NET 9.0 (C# / WinForms) — all editions
*   **Web Orchestration**: Apache 2.4 *(Lite & Pro)* · RoadRunner *(Go!)*

### Database & Storage
*   **RDBMS**: MySQL 8.0 *(Lite & Pro)* · MariaDB 11 and PostgreSQL 17 with pgvector *(Go!)*
*   **Tools**: phpMyAdmin *(all editions)* · pgAdmin 4 Desktop Manager *(Go!)*

### Runtimes
*   **PHP**: 7.4 *(Lite)* · 5.6 / 7.4 / 8.2 / 8.4 *(Pro)* · 8.4+ *(Go!)*
*   **Runtime Libraries**: Bundled VC++ 2012-2022 redistributables

---

## 📦 Version Comparison

| Feature | Lite | Pro/Advance | Go! Edition |
| :--- | :---: | :---: | :---: |
| **Web Server** | Apache 2.4 | Apache 2.4 | **RoadRunner** |
| **PHP Support** | Single (7.4) | Multi (5.6 / 7.4 / 8.2 / 8.4) | **Modern (8.4+)** |
| **DB (SQL)** | MySQL 8.0 | MySQL 8.0 | **MariaDB 11** |
| **DB (Postgre)** | ❌ | ❌ | **PostgreSQL 17** |
| **AI Hub** | ❌ | ❌ | **Integrated** |
| **Monitoring** | Basic | Basic | **Real-time Stats** |
| **Environment** | Common Path | Isolated Paths | **High-Concurrency** |

---

## 🏗️ Getting Started

### Prerequisites
*   Windows 10 / 11 (64-bit)
*   .NET 9.0 Desktop Runtime

### Installation
1.  Download the professional installer for your preferred edition.
2.  Run the setup and follow the high-speed deployment wizard.
3.  Launch **DesktopServer Manager** from the desktop or system tray.

> [!IMPORTANT]
> **Source Builds**: If you are building from source, please refer to the **[BUILD_GUIDE.md](BUILD_GUIDE.md)** for binary dependency instructions (Apache, PHP, etc.), as these are not bundled in the core repository due to size constraints.

---

## 💖 Support the Project

If **Monrak DesktopServer** helps you build better apps faster, consider supporting its development. Your contributions help keep the project alive and growing.

<div align="center">
  <img src="docs/images/promptpay.png" width="300" alt="Support via PromptPay">
  <p><i>Support via PromptPay (Thailand)</i></p>
</div>

---

## 📜 License

Released under the **[MIT License](LICENSE.txt)** — free to use, modify and distribute, provided the copyright notice is kept.

Copyright © 2026 **Kirati Kittikhun**
Developed for professional excellence by [Monrak Net Technology](https://github.com/monraknet).

> Bundled third-party components (Apache, PHP, MySQL, MariaDB, PostgreSQL, RoadRunner, phpMyAdmin) each keep their own licences.

---
> [!NOTE]
> Optimized for local development. For production-grade security, follow the included configuration hardening guides for Apache and MySQL.
