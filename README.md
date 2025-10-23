# Faculdade de Informática e Administração Paulista — FIAP
**Matéria:** Advanced Business Development with .NET  
**Aluno:** Vinicius Banciela Breda Lopes  
**RM:** 558117  
**Turma:** 2TDSPW

---

# Hashtag Generator API (Minimal API + Ollama)

API minimalista em **.NET 8** que consome a **Ollama REST API** rodando localmente para gerar **N hashtags** a partir de um texto de entrada.  
O projeto demonstra:
- Consumo de LLM local via `POST /api/generate` com `stream=false`;
- **Structured outputs** com `json_schema` quando suportado;
- **Fallback automático** para `format: "json"` (compatível com qualquer modelo);
- Validações e mensagens de erro úteis;
- Testes de **caixa-preta** com `Requests.http` (VS Code REST Client).

---

## Estrutura do Projeto

A solução foi organizada seguindo boas práticas de desenvolvimento em .NET,
separando os modelos de dados (Models e DTOs) do código principal,
incluindo um arquivo .http para testes via VS Code e um README.md explicativo.

```
HASHTAGGENERATORAPI-DOTNET_OLLAMA/
├── HashtagGenerator.Api.sln       # Arquivo de solução (.sln) — agrupa e referencia o projeto principal (em caso de expansão futura)
├── HashtagGenerator.Api.csproj    # Arquivo de configuração do projeto (.NET 8 Web API)
├── Program.cs                     # Código principal da Minimal API (validações, integração e endpoint /hashtags)
│
├── Models/
│   ├── HashtagRequest.cs          # Modelo de entrada (text, count, model)
│   ├── HashtagResponse.cs         # Modelo de saída em sucesso (model, count, hashtags)
│   └── ErrorResponse.cs           # Modelo de saída em erro (message)
│
├── OllamaDTOs/
│   ├── OllamaRequest.cs           # Estrutura de requisição enviada ao Ollama
│   └── OllamaResponse.cs          # Estrutura de resposta retornada pelo Ollama
│
├── Requests.http                  # Arquivo de testes (VS Code REST Client)
└── README.md                      # Documentação explicativa e guia de execução


```
Arquivos auxiliares (appsettings.json, appsettings.Development.json, launchSettings.json, .gitignore) estão presentes no projeto, mas foram omitidos da estrutura para simplificação visual.
Esses arquivos são responsáveis por configurações de ambiente, perfis de execução e regras de versionamento, e não afetam a lógica principal da API.

## Pré-requisitos

| Item | Versão | Observações |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ | Necessário para compilar/rodar a API |
| [Ollama](https://ollama.com/download) | 0.12.x | Servidor local em `http://localhost:11434` |
| Modelos Ollama | `llama3.2:3b` ou outros | baixar com `ollama pull llama3.2:3b` |
| [VS Code](https://code.visualstudio.com/download) + extensão | [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) (`humao.rest-client`) | Envia requisições via `Requests.http` |


---

## Como executar

1. **Suba (ou confirme) o Ollama**
   ```bash
   # se necessário - inicia o servidor Ollama
   ollama serve
   # ou apenas teste - verifica se está rodando e lista os modelos 
   curl http://localhost:11434/api/tags
   ```

2. **Clone o projeto, Abra o VS Code e Rode o projeto**
    ```bash
    git clone https://github.com/vinibanciela/CP5-HashtagGeneratorAPI-dotNet_Ollama.git
    cd HashtagGeneratorAPI-dotNet_Ollama
    code .
    dotnet run
    ```
    (A API irá iniciar, em http://localhost:5172)

3. **Testes rápidos**

Via arquivo `.http` (VS Code REST Client ou JetBrains)

Abra `Requests.http` e clique em `Send Request`.


## Critérios de aceitação atendidos

* Consome Ollama via **HttpClient** com `stream=false` e `structured outputs (JSON Schema)`.
*  **POST /hashtags** recebe `{ text, count, model }` e retorna `200 OK` com `{ model, count, hashtags[] }`.
* Gera **exatamente N** hashtags, iniciando com `#`, **sem espaços** e **sem duplicatas** (há sanitização pós-modelo).
* Arquivo **Requests.http** incluído para teste com REST CLIENT.
* Quando **`count` é omitido**, usa **padrão = 10**. Quando `count > 30`, retorna **400** com mensagem útil.
* Quando **`model` é omitido**, usa padrão `ollama pull llama3.2:3b`.
* Possibilidade de utilizar outros modelos com **Fallback automático** para `format: "json"`.
* Em erros (entrada inválida, indisponibilidade do Ollama, resposta inválida, retorna **400** com mensagem clara.

