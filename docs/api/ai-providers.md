# AI Providers API

Endpoints for retrieving available AI providers.

[← Back to API Reference](README.md)

---

## Overview

AI providers are the backends that generate AI responses. Each group can be configured to use a specific provider.

### Supported Providers

| Provider         | Name     | Description              |
| ---------------- | -------- | ------------------------ |
| Google Gemini    | `gemini` | Google's multimodal AI   |
| Anthropic Claude | `claude` | Anthropic's assistant AI |
| OpenAI           | `openai` | GPT models               |
| xAI Grok         | `grok`   | xAI's conversational AI  |

---

## Endpoints

| Method | Endpoint                                 | Description        | Auth     |
| ------ | ---------------------------------------- | ------------------ | -------- |
| GET    | [/ai-providers](#get-ai-providers)       | List all providers | Required |
| GET    | [/ai-providers/:id](#get-ai-providersid) | Get provider by ID | Required |

---

## GET /ai-providers

List all available (enabled) AI providers.

### Request

```
GET /api/ai-providers
Authorization: Bearer <access_token>
```

### Response (200 OK)

```json
[
  {
    "id": "770e8400-e29b-41d4-a716-446655440000",
    "name": "gemini",
    "displayName": "Google Gemini",
    "defaultModel": "gemini-1.5-pro",
    "defaultTemperature": 0.7,
    "maxTokensLimit": 128000
  },
  {
    "id": "771e8400-e29b-41d4-a716-446655440000",
    "name": "claude",
    "displayName": "Anthropic Claude",
    "defaultModel": "claude-3-sonnet",
    "defaultTemperature": 0.7,
    "maxTokensLimit": 200000
  },
  {
    "id": "772e8400-e29b-41d4-a716-446655440000",
    "name": "openai",
    "displayName": "OpenAI",
    "defaultModel": "gpt-4o",
    "defaultTemperature": 0.7,
    "maxTokensLimit": 128000
  },
  {
    "id": "773e8400-e29b-41d4-a716-446655440000",
    "name": "grok",
    "displayName": "xAI Grok",
    "defaultModel": "grok-2",
    "defaultTemperature": 0.7,
    "maxTokensLimit": 131072
  }
]
```

### Response Fields

| Field              | Type   | Description                                |
| ------------------ | ------ | ------------------------------------------ |
| id                 | string | UUID - use when updating group AI settings |
| name               | string | Short name for @mentions (e.g., `@gemini`) |
| displayName        | string | Human-readable name for UI                 |
| defaultModel       | string | Default model used                         |
| defaultTemperature | number | Creativity setting (0.0-1.0)               |
| maxTokensLimit     | number | Maximum context size                       |

### Errors

| Status | Error        | Description              |
| ------ | ------------ | ------------------------ |
| 401    | Unauthorized | Missing or invalid token |

---

## GET /ai-providers/:id

Get a specific AI provider by ID.

### Request

```
GET /api/ai-providers/770e8400-e29b-41d4-a716-446655440000
Authorization: Bearer <access_token>
```

### Response (200 OK)

```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "name": "gemini",
  "displayName": "Google Gemini",
  "defaultModel": "gemini-1.5-pro",
  "defaultTemperature": 0.7,
  "maxTokensLimit": 128000
}
```

### Errors

| Status | Error        | Description              |
| ------ | ------------ | ------------------------ |
| 401    | Unauthorized | Missing or invalid token |
| 404    | NotFound     | Provider not found       |

---

## Frontend Implementation

### AI Provider Selector

```typescript
function AiProviderSelector({
  value,
  onChange,
}: {
  value: string;
  onChange: (id: string) => void;
}) {
  const [providers, setProviders] = useState<AiProvider[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/ai-providers", {
      headers: { Authorization: `Bearer ${accessToken}` },
    })
      .then((r) => r.json())
      .then((data) => {
        setProviders(data);
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <div>Loading providers...</div>;
  }

  return (
    <div className="provider-selector">
      {providers.map((provider) => (
        <label
          key={provider.id}
          className={`provider-option ${
            value === provider.id ? "selected" : ""
          }`}
        >
          <input
            type="radio"
            name="provider"
            value={provider.id}
            checked={value === provider.id}
            onChange={() => onChange(provider.id)}
          />
          <div className="provider-info">
            <span className="provider-name">{provider.displayName}</span>
            <span className="provider-model">{provider.defaultModel}</span>
          </div>
        </label>
      ))}
    </div>
  );
}
```

### Provider Display Component

```typescript
function ProviderBadge({ provider }: { provider: AiProvider }) {
  const icons: Record<string, string> = {
    gemini: "🔷",
    claude: "🟠",
    openai: "🟢",
    grok: "⚫",
  };

  return (
    <span className="provider-badge">
      {icons[provider.name] || "🤖"} {provider.displayName}
    </span>
  );
}
```

### Caching Providers

Since providers rarely change, cache them:

```typescript
let providersCache: AiProvider[] | null = null;

async function getProviders(): Promise<AiProvider[]> {
  if (providersCache) {
    return providersCache;
  }

  const response = await fetch("/api/ai-providers", {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

  providersCache = await response.json();
  return providersCache;
}

// Get provider by ID from cache
async function getProviderById(id: string): Promise<AiProvider | undefined> {
  const providers = await getProviders();
  return providers.find((p) => p.id === id);
}

// Get provider by name for @mention highlighting
async function getProviderByName(
  name: string
): Promise<AiProvider | undefined> {
  const providers = await getProviders();
  return providers.find((p) => p.name === name.toLowerCase());
}
```

---

## AI Provider Response Schema

```typescript
interface AiProviderResponse {
  id: string; // UUID
  name: string; // Short name (gemini, claude, etc.)
  displayName: string; // Human-readable name
  defaultModel: string; // Model identifier
  defaultTemperature: number; // 0.0 - 1.0
  maxTokensLimit: number; // Max context tokens
}
```
