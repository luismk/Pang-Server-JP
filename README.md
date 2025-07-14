# Pang-Server-JP

Servidor baseado no código de Acrisio (SuperSS Dev) — reconstruído e adaptado em C#.

> ⚠️ **Este projeto é fornecido como base de estudo. Você é livre para modificar, adaptar ou utilizar como quiser.**

---
 ### 📌 Visão Geral

Este projeto simula os principais componentes de um servidor PangYa:

- **LoginServer** – Autenticação de jogadores.
- **MessengerServer** – Sistema de mensagens e amigos, guild.
- **GameServer** – Lobby, salas e partidas.
- **AuthServer** – Sicronia entre os servidores, dados, envio e conversa entre si.

É compatível com o cliente japonês **ProjectG JP versão 972.00 ou superior**.

---
### ✅ Status do Projeto

| Componente       | Progresso |
|------------------|-----------|
| GameServer       | 85%       |
| MessengerServer  | 99%       |
| LoginServer      | 100%      |
| AuthServer       | 100%      |

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

| API                        | Função principal                                                                      |
|----------------------------|---------------------------------------------------------------------------------------|
| **PangyaAPI.Network**      | Gerencia conexões TCP, sessões, buffers, envio/recebimento e tratamento de pacotes.   |
| **PangyaAPI.SQL**          | Interface de acesso ao banco de dados (SQL Server), comandos e respostas assíncronas. |
| **PangyaAPI.IFF.JP**       | Manipula os arquivos IFF do cliente japonês (itens, personagens, cursos etc.).        |
| **PangyaAPI.Utilities**    | Ferramentas auxiliares: Log, enums, config `.ini`, criptografia, estrutura de erros.  |

Essa separação torna o código mais limpo, reutilizável e facilita a manutenção e expansão.

### 🚀 Como começar

> **Nota:** Eu não vou ensinar como conectar o servidor ao cliente, mas...  
> 💡 **Dica:** leia os comentários no código — cada parte tem explicações úteis para te guiar!

---

### 🧠 Dicas rápidas

- Confira os arquivos `.ini` para ajustar configurações de porta, IP e nome do servidor.
- Observe o `pangya_packet_handle.cs` para entender como os pacotes são tratados.
- Observe o `SessionManager.cs` para entender como os jogadores são tratados.
- Use os logs no console para debugar conexões e autenticações.

---

### 🖼️ Capturas de Tela

   [![Test Stress](https://img.youtube.com/vi/bshhw92QnSQ/0.jpg)](https://www.youtube.com/watch?v=bshhw92QnSQ)
   [![Test Stress 2](https://img.youtube.com/vi/VhF3byU_azc/0.jpg)](https://www.youtube.com/watch?v=VhF3byU_azc) 
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
