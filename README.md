# Pang-Server-JP

Servidor baseado no código de Acrisio (SuperSS Dev) — reconstruído e adaptado em C#.

> ⚠️ **Este projeto é fornecido como base de estudo. Você é livre para modificar, adaptar ou utilizar como quiser.**

---
 ### 📌 Visão Geral

Este projeto simula os principais componentes de um servidor PangYa:

- **LoginServer** – Autenticação de jogadores.
- **MessengerServer** – Sistema de mensagens e amigos.
- **GameServer** – Lobby, salas e partidas.

É compatível com o cliente japonês **ProjectG JP versão 972.00 ou superior**.

---
### ✅ Status do Projeto

| Componente       | Progresso |
|------------------|-----------|
| GameServer       | 15%       |
| MessengerServer  | 98%       |
| LoginServer      | 100%        |

---

### 🧩 Requisitos

Você vai precisar de alguns programas e ferramentas:

- [Visual Studio](https://visualstudio.microsoft.com/pt-br/) – para compilar o projeto.
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) – para gerenciar o banco de dados.
- Cliente do **Pangya JP** – compatível com versão **972.00 ou superior** (ProjectG JP).

---
---

### 🧱 Arquitetura do Projeto

O Pang-Server-JP é dividido em 5 principais bibliotecas (`PangyaAPI`) que organizam o código de forma modular:

| API                         | Função principal                                                                 |
|----------------------------|----------------------------------------------------------------------------------|
| **PangyaAPI.Network**      | Gerencia conexões TCP, sessões, buffers, envio/recebimento e tratamento de pacotes. |
| **PangyaAPI.SQL**          | Interface de acesso ao banco de dados (SQL Server), comandos e respostas assíncronas. |
| **PangyaAPI.IFF.JP**       | Manipula os arquivos IFF do cliente japonês (itens, personagens, cursos etc.).     |
| **PangyaAPI.Discord**      | Integração com Discord para logs, status do servidor ou notificações.             |
| **PangyaAPI.Utilities**    | Ferramentas auxiliares: logging, enums, config `.ini`, criptografia, estrutura de erros. |

Essa separação torna o código mais limpo, reutilizável e facilita a manutenção e expansão.

### 🚀 Como começar

> **Nota:** Eu não vou ensinar como conectar o servidor ao cliente, mas...  
> 💡 **Dica:** leia os comentários no código — cada parte tem explicações úteis para te guiar!

---

### 🧠 Dicas rápidas

- Confira os arquivos `.ini` para ajustar configurações de porta, IP e nome do servidor.
- Observe o `SessionManager` e `PacketHandler` para entender como os pacotes são tratados.
- Use os logs no console para debugar conexões e autenticações.

---

### 🖼️ Capturas de Tela

![pangya_001](https://cdn.discordapp.com/attachments/521180240542826498/1376218557020504064/image-12.png?ex=683486e8&is=68333568&hm=95a745f9d436116f5f4a7d9c44de4aacef9b056a66ecfec1cf6644387e536b1a&)
![pangya_002](https://cdn.discordapp.com/attachments/521180240542826498/1376218557020504064/image-12.png?ex=683486e8&is=68333568&hm=95a745f9d436116f5f4a7d9c44de4aacef9b056a66ecfec1cf6644387e536b1a&)
---

### 👨‍💻 Autores

| Nome           | Função         | Projeto                          |
|----------------|----------------|----------------------------------|
| **Luis MK**    | Criador        | [Dev Pangya Unogames](https://github.com/luismk)  
| **Eric Antonio** | Contribuidor | [Old ADM Pangya Unogames](https://github.com/eantoniobr)
| **Narwyn**     | Contribuidor   | [Pangya Reborn](https://github.com/Narwyn)

---

### 📜 Licença

Este projeto não possui uma licença formal. Use por sua conta e risco.  
**Não recomendado para uso comercial sem entendimento profundo do código.**

---
