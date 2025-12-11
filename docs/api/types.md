# TypeScript Types

Complete TypeScript type definitions for the AI Group Chat API.

You can copy these types directly into your frontend project.

---

## Quick Start

Create a file `src/types/api.ts` and paste these types:

```typescript
// =============================================================================
// AI GROUP CHAT API - TYPESCRIPT TYPES
// =============================================================================

// -----------------------------------------------------------------------------
// AUTH TYPES
// -----------------------------------------------------------------------------

export interface RegisterRequest {
  email: string;
  userName: string;
  displayName: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ConfirmEmailRequest {
  email: string;
  token: string;
}

export interface ResendConfirmationRequest {
  email: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string; // ISO 8601
  user: UserDto;
}

export interface UserDto {
  id: string;
  email: string;
  userName: string;
  displayName: string;
}

export interface MessageResponse {
  message: string;
}

// -----------------------------------------------------------------------------
// USER TYPES
// -----------------------------------------------------------------------------

export interface UserResponse {
  id: string;
  email: string;
  userName: string;
  displayName: string;
  createdAt: string; // ISO 8601
}

// -----------------------------------------------------------------------------
// GROUP TYPES
// -----------------------------------------------------------------------------

export interface CreateGroupRequest {
  name: string;
}

export interface UpdateGroupRequest {
  name: string;
}

export interface UpdateAiSettingsRequest {
  aiMonitoringEnabled?: boolean;
  aiProviderId?: string;
}

export interface GroupResponse {
  id: string;
  name: string;
  createdById: string;
  aiMonitoringEnabled: boolean;
  aiProviderId: string;
  aiProvider: AiProviderResponse;
  createdAt: string; // ISO 8601
  updatedAt: string; // ISO 8601
  members: GroupMemberResponse[];
}

// -----------------------------------------------------------------------------
// GROUP MEMBER TYPES
// -----------------------------------------------------------------------------

export type MemberRole = "Owner" | "Admin" | "Member";

export interface AddMemberRequest {
  userId: string;
}

export interface UpdateMemberRoleRequest {
  role: "Admin" | "Member"; // Cannot set to Owner via this endpoint
}

export interface TransferOwnershipRequest {
  newOwnerUserId: string;
}

export interface GroupMemberResponse {
  userId: string;
  userName: string;
  displayName: string;
  role: MemberRole;
  joinedAt: string; // ISO 8601
}

// -----------------------------------------------------------------------------
// MESSAGE TYPES
// -----------------------------------------------------------------------------

export type SenderType = "user" | "ai";

export interface SendMessageRequest {
  content: string;
}

export interface MessageItemResponse {
  id: string;
  groupId: string;
  senderId: string | null;
  senderUserName: string | null;
  senderDisplayName: string | null;
  senderType: SenderType;
  content: string;
  attachmentUrl: string | null;
  attachmentType: string | null;
  attachmentName: string | null;
  createdAt: string; // ISO 8601
}

// -----------------------------------------------------------------------------
// AI PROVIDER TYPES
// -----------------------------------------------------------------------------

export interface AiProviderResponse {
  id: string;
  name: string; // For @mentions: gemini, claude, openai, grok
  displayName: string;
  defaultModel: string;
  defaultTemperature: number;
  maxTokensLimit: number;
}

// -----------------------------------------------------------------------------
// PAGINATION
// -----------------------------------------------------------------------------

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export type PaginatedMessages = PaginatedResponse<MessageItemResponse>;

// -----------------------------------------------------------------------------
// ERRORS
// -----------------------------------------------------------------------------

export interface ApiError {
  error: string;
  message: string;
  details?: string[];
}

export type ApiErrorType =
  | "ValidationError"
  | "InvalidCredentials"
  | "InvalidToken"
  | "EmailNotConfirmed"
  | "Forbidden"
  | "NotFound";

// -----------------------------------------------------------------------------
// SIGNALR EVENTS - GROUP CHANNEL
// -----------------------------------------------------------------------------

export interface UserTypingEvent {
  groupId: string;
  userId: string;
  userName: string;
  displayName: string;
}

export interface UserStoppedTypingEvent {
  groupId: string;
  userId: string;
}

export interface AiTypingEvent {
  groupId: string;
  providerId: string;
  providerName: string;
}

export interface AiStoppedTypingEvent {
  groupId: string;
  providerId: string;
}

export interface MemberJoinedEvent {
  groupId: string;
  userId: string;
  userName: string;
  displayName: string;
  role: MemberRole;
  joinedAt: string;
}

export interface MemberLeftEvent {
  groupId: string;
  userId: string;
  displayName: string;
  leftAt: string;
}

export interface MemberRoleChangedEvent {
  groupId: string;
  userId: string;
  displayName: string;
  oldRole: MemberRole;
  newRole: MemberRole;
}

export interface AiSettingsChangedEvent {
  groupId: string;
  aiMonitoringEnabled: boolean;
  aiProviderId: string | null;
  aiProviderName: string | null;
  changedByName: string;
  changedAt: string;
}

// -----------------------------------------------------------------------------
// SIGNALR EVENTS - PERSONAL CHANNEL
// -----------------------------------------------------------------------------

export interface AddedToGroupEvent {
  groupId: string;
  groupName: string;
  addedByName: string;
  role: MemberRole;
  addedAt: string;
}

export interface RemovedFromGroupEvent {
  groupId: string;
  groupName: string;
  removedAt: string;
}

export interface RoleChangedEvent {
  groupId: string;
  groupName: string;
  oldRole: MemberRole;
  newRole: MemberRole;
  changedByName: string;
  changedAt: string;
}

export interface NewMessageNotificationEvent {
  groupId: string;
  groupName: string;
  messageId: string;
  senderName: string;
  preview: string;
  sentAt: string;
}

export interface GroupActivityEvent {
  groupId: string;
  groupName: string;
  activityType: string;
  timestamp: string;
  preview: string | null;
  actorName: string | null;
}

export interface UserOnlineEvent {
  userId: string;
  displayName: string;
  onlineAt: string;
}

export interface UserOfflineEvent {
  userId: string;
  displayName: string;
  offlineAt: string;
}
```

---

## Type Helpers

Utility types and type guards:

```typescript
// -----------------------------------------------------------------------------
// TYPE GUARDS
// -----------------------------------------------------------------------------

export function isAiMessage(message: MessageItemResponse): boolean {
  return message.senderType === "ai";
}

export function isUserMessage(message: MessageItemResponse): boolean {
  return message.senderType === "user";
}

export function isOwner(member: GroupMemberResponse): boolean {
  return member.role === "Owner";
}

export function isAdmin(member: GroupMemberResponse): boolean {
  return member.role === "Admin" || member.role === "Owner";
}

// -----------------------------------------------------------------------------
// PERMISSION HELPERS
// -----------------------------------------------------------------------------

export function canManageMembers(role: MemberRole): boolean {
  return role === "Owner" || role === "Admin";
}

export function canChangeRoles(role: MemberRole): boolean {
  return role === "Owner";
}

export function canDeleteGroup(role: MemberRole): boolean {
  return role === "Owner";
}

export function canUpdateAiSettings(role: MemberRole): boolean {
  return role === "Owner" || role === "Admin";
}

export function canRemoveMember(
  actorRole: MemberRole,
  targetRole: MemberRole
): boolean {
  if (actorRole === "Owner") {
    return targetRole !== "Owner";
  }
  if (actorRole === "Admin") {
    return targetRole === "Member";
  }
  return false;
}

// -----------------------------------------------------------------------------
// DATE HELPERS
// -----------------------------------------------------------------------------

export function formatRelativeTime(isoString: string): string {
  const date = new Date(isoString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMins / 60);
  const diffDays = Math.floor(diffHours / 24);

  if (diffMins < 1) return "just now";
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;

  return date.toLocaleDateString();
}

// -----------------------------------------------------------------------------
// MESSAGE HELPERS
// -----------------------------------------------------------------------------

export function getMessagePreview(content: string, maxLength = 50): string {
  if (content.length <= maxLength) return content;
  return content.substring(0, maxLength - 3) + "...";
}

export function extractMention(content: string): string | null {
  const match = content.match(/@(gemini|claude|openai|grok)/i);
  return match ? match[1].toLowerCase() : null;
}
```

---

## SignalR Event Map

Type-safe event handling:

```typescript
// -----------------------------------------------------------------------------
// SIGNALR EVENT MAP
// -----------------------------------------------------------------------------

export interface SignalREventMap {
  // Group channel events
  MessageReceived: MessageItemResponse;
  UserTyping: UserTypingEvent;
  UserStoppedTyping: UserStoppedTypingEvent;
  AiTyping: AiTypingEvent;
  AiStoppedTyping: AiStoppedTypingEvent;
  MemberJoined: MemberJoinedEvent;
  MemberLeft: MemberLeftEvent;
  MemberRoleChanged: MemberRoleChangedEvent;
  AiSettingsChanged: AiSettingsChangedEvent;

  // Personal channel events
  AddedToGroup: AddedToGroupEvent;
  RemovedFromGroup: RemovedFromGroupEvent;
  RoleChanged: RoleChangedEvent;
  NewMessageNotification: NewMessageNotificationEvent;
  GroupActivity: GroupActivityEvent;
  UserOnline: UserOnlineEvent;
  UserOffline: UserOfflineEvent;
}

export type SignalREventName = keyof SignalREventMap;
```

---

## Usage Example

```typescript
import type {
  AuthResponse,
  GroupResponse,
  MessageItemResponse,
  MemberRole,
  isAiMessage,
  canManageMembers,
} from "./types/api";

// Type-safe API call
async function login(email: string, password: string): Promise<AuthResponse> {
  const response = await fetch("/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  return response.json();
}

// Type-safe message rendering
function Message({ message }: { message: MessageItemResponse }) {
  if (isAiMessage(message)) {
    return <AiBubble content={message.content} />;
  }
  return <UserBubble message={message} />;
}

// Type-safe permission check
function canUserRemove(userRole: MemberRole, targetRole: MemberRole) {
  return canManageMembers(userRole) && canRemoveMember(userRole, targetRole);
}
```
