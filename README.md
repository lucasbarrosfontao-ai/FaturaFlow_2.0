# FaturaFlow 2.0 🚀

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Docker](https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/Rabbitmq-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![MySQL](https://img.shields.io/badge/mysql-4479A1.svg?style=for-the-badge&logo=mysql&logoColor=white)

> **Projeto de Prova de Aptidão Profissional (PAP)**  
> **Aluno:** Lucas Hariel de Barros Fontão  
> **Turma:** 3B1 - Informática de Gestão (2025/2026)  
> **Escola:** Escola Secundária Camilo Castelo Branco, Vila Real, Portugal.

---

## 📋 Sobre o Projeto

O **FaturaFlow 2.0** é uma plataforma web robusta para gestão de faturação e clientes. Diferente de sistemas tradicionais, esta versão foi reconstruída utilizando **Clean Architecture** e **Domain-Driven Design (DDD)**, focando na escalabilidade e desacoplamento de serviços.

O sistema permite a gestão completa de clientes, fornecedores e produtos, além da emissão de faturas em PDF com envio assíncrono por e-mail via filas de processamento.

## 🛠️ Tecnologias e Arquitetura

O projeto foi desenvolvido seguindo padrões de indústria modernos:

*   **Backend:** ASP.NET Core 8 (C#)
*   **Frontend:** Razor Pages (Server-Side Rendering)
*   **Base de Dados:** MySQL (via Pomelo Entity Framework Core)
*   **Mensageria:** RabbitMQ (para processamento assíncrono de tarefas)
*   **Containerização:** Docker & Docker Compose
*   **Geração de Relatórios:** QuestPDF
*   **Serviços de Fundo:** Hosted Services (Worker)

### 🏗️ Estrutura da Solução (DDD)

A solução está dividida em camadas para garantir a separação de responsabilidades:

1.  **Domain:** Entidades, Value Objects e Interfaces (O "coração" do negócio).
2.  **Infrastructure:** Implementação de Repositórios, acesso a Dados (EF Core), serviços de E-mail e PDF.
3.  **Application:** Casos de uso e lógica de aplicação.
4.  **Presentation (Web):** Interface do utilizador em Razor Pages.
5.  **Worker:** Serviço em background que consome filas do RabbitMQ para tarefas pesadas (envio de e-mails, geração de PDFs).

## ✨ Funcionalidades Principais

*   ✅ **CRUD Completo:** Gestão de Clientes, Fornecedores e Produtos.
*   ✅ **Faturação:** Criação de faturas com cálculo automático de IVA e totais.
*   ✅ **PDF Automático:** Geração de faturas em PDF com layout profissional.
*   ✅ **Envio de E-mail Assíncrono:** Ao emitir uma fatura, o sistema coloca um pedido na fila (RabbitMQ) e o Worker processa o envio do e-mail com o PDF em anexo, sem travar a interface do utilizador.
*   ✅ **Recuperação de Senha:** Fluxo seguro de envio de códigos de verificação.
*   ✅ **Tratamento de Erros:** Validações de domínio (Value Objects) e tratamento de duplicidade de dados (NIF/Email únicos).

## 🚀 Como Executar o Projeto

Este projeto utiliza **Docker**, o que torna a execução extremamente simples. Não é necessário instalar o MySQL ou RabbitMQ manualmente.

### Pré-requisitos
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado.
*   Git.

### Passo a Passo

1.  **Clonar o repositório:**
    ```bash
    git clone https://github.com/lucasbarrosfontao-ai/FaturaFlow_2.0.git
    cd FaturaFlow_2.0
    ```

2.  **Configurar Variáveis de Ambiente:**
    Crie um arquivo `.env` na raiz do projeto (baseado nesse exemplo abaixo ou no arquivo .env-example) e configure as suas credenciais (ex: Mailtrap):
    ```env
    SMTP_HOST=smtp.mailtrap.io
    SMTP_PORT=2525
    SMTP_USER=seu_usuario_mailtrap
    SMTP_PASS=sua_senha_mailtrap
    DB_PASSWORD=sua_senha_mysql
    RABBITMQ_USER=guest
    RABBITMQ_PASSWORD=guest
    ```

3.  **Subir os Containers:**
    Na raiz do projeto, execute:
    ```bash
    docker compose up --build (para ver logs diretamente no console)
    docker compose up -d --build (para não ver os logs diratemente no console)
    ```

4.  **Aceder à Aplicação:**
    *   **Web App:** http://localhost:8080 (ou a porta configurada no docker-compose)
    *   **RabbitMQ Manager:** http://localhost:15672 (Login: guest / guest)
    *   **Utilizador e Senha padrão para entrada no programa:** admin/admin (minúscolos) (pode ser configurado dentro do programa na parte de utilizadores) 

---

## 📞 Contacto

**Lucas Hariel de Barros Fontão**  
📧 a36869@esccbvr.pt Ou lucasbarrosfontao@gmail.com Ou lucashariel689@gmail.com  

---
*Este projeto foi desenvolvido exclusivamente para fins académicos no âmbito da PAP.*
*Projeto sujeito a atualizações futuras*