# Faculdade de Informática e Administração Paulista — FIAP
**Matéria:** Advanced Business Development with .NET  
**Aluno:** Vinicius Banciela Breda Lopes  
**RM:** 558117  
**Turma:** 2TDSPW

---

# Hashtag Generator API (Minimal API + Ollama)

API minimalista em **.NET 8** que consome a **Ollama REST API** rodando localmente para gerar **N hashtags** a partir de um texto de entrada.  
O projeto demonstra:
- Consumo de LLM local via `POST /api/generate`;
- **Structured outputs** com `json_schema` quando suportado;
- **Fallback automático** para `format: "json"` (compatível com qualquer modelo);
- Validações e mensagens de erro úteis;
- Testes de **caixa-preta** com `Requests.http` (VS Code REST Client).

---

## Estrutura do Projeto

A solução foi organizada seguindo as melhores práticas, separando os modelos de dados (DTOs) do arquivo principal.

```
HASHTAGGENERATOR.API/
├── HashtagGenerator.Api.sln
├── HashtagGenerator.Api.csproj
├── Program.cs
├── Models/
│   ├── HashtagRequest.cs
│   ├── HashtagResponse.cs
│   └── ErrorResponse.cs
├── OllamaDTOs/
│   ├── OllamaRequest.cs
│   └── OllamaResponse.cs
├── Requests.http
└── README.md

```

## Pré-requisitos

| Item | Versão | Observações |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ | Necessário para compilar/rodar a API |
| [Ollama](https://ollama.com/download) | 0.12.x | Servidor local em `http://localhost:11434` |
| Modelos Ollama | `llama3.2:3b` ou outros | `ollama pull llama3.2:3b` |
| VS Code + extensão | REST Client (`humao.rest-client`) | Envia requisições via `Requests.http` |

> Verifique o Ollama:  
> `curl http://localhost:11434/api/tags`  
> Deve listar os modelos instalados (ex.: `llama3.2:3b`, outros).

---

## Como executar

1. **Suba (ou confirme) o Ollama**
   ```bash
   # se necessário
   ollama serve
   # ou apenas teste
   curl http://localhost:11434/api/tags
   ```

2. **Clonar o projeto, Abrir o VS Code e Rodar o Projeto**
    ```bash
    git clone https://github.com/vinibanciela/CP5-HashtagGeneratorAPI-dotNet_Ollama.git
    cd HashtagGeneratorAPI-dotNet_Ollama
    code .
    dotnet run
    ```
    (A API irá iniciar,  em http://localhost:5172)

4. **Testes rápidos**

Via arquivo `.http` (VS Code REST Client ou JetBrains)

Abra `Requests.http` e clique em `Send Request`.


## Critérios de aceitação atendidos

* **POST /hashtags** recebe `{ text, count, model }` e retorna `200 OK` com `{ model, count, hashtags[] }`.
* Gera **exatamente N** hashtags, iniciando com `#`, **sem espaços** e **sem duplicatas** (há sanitização pós-modelo).
* Quando **`count` é omitido**, usa **padrão = 10**. Quando `count > 30`, retorna **400** com mensagem útil.
* Em erros (entrada inválida, indisponibilidade do Ollama, resposta inválida, etc), retorna **400** com mensagem clara.
* Consome Ollama via **HttpClient** com `stream=false` e **structured outputs (JSON Schema).
* Arquivo **Requests.http** incluído poara teste com REST CLIENT.
