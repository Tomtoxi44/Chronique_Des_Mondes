# UI/UX Design Prompt - Chronique des Mondes

## Project Overview

**Chronique des Mondes** is a tabletop RPG management platform for Game Masters and players. It allows users to manage campaigns, characters, real-time game sessions, combat systems, and dice rolling. The platform supports multiple game systems: D&D 5e, Generic, and Skyrim.

---

## Brand Identity & Color Palette

Based on the logo (see `logo.png` in this directory): medieval open book, D20 dice, dragon, castle silhouette, crescent moon, quill with golden/bronze ornate frame.

### Primary Colors
| Color | Hex | Usage |
|-------|-----|-------|
| **Royal Gold** | `#C9A227` | Primary accent, buttons, highlights, logo elements |
| **Deep Bronze** | `#8B6914` | Secondary accent, borders, icons |
| **Crimson Red** | `#8B2942` | D20 dice accent, important actions, alerts |

### Dark Theme
| Color | Hex | Usage |
|-------|-----|-------|
| **Night Sky** | `#0D1B2A` | Main background |
| **Dark Slate** | `#1B2838` | Cards, containers |
| **Charcoal** | `#2C3E50` | Secondary backgrounds, hover states |
| **Soft White** | `#E8E6E3` | Primary text |
| **Muted Gold** | `#BFA76F` | Secondary text, labels |

### Light Theme
| Color | Hex | Usage |
|-------|-----|-------|
| **Parchment** | `#F5F0E6` | Main background (old paper feel) |
| **Warm White** | `#FDFCFA` | Cards, containers |
| **Light Sand** | `#E8DFD0` | Secondary backgrounds |
| **Dark Ink** | `#2C2416` | Primary text |
| **Bronze Brown** | `#5C4A32` | Secondary text, labels |

---

## Design Requirements

### General Guidelines
- **Clean and professional** look, not cluttered
- **Intuitive navigation** - users should understand where to click without thinking
- **Responsive design** - works on desktop, tablet, and mobile
- **Dark/Light mode toggle** - smooth transition between themes
- **Fantasy aesthetic** - subtle medieval/fantasy touches without being cartoonish
- **Accessibility** - WCAG 2.1 AA compliant (contrast ratios, focus states)

### Typography
- **Headings**: A serif font with slight medieval character (e.g., Cinzel, Cormorant Garamond)
- **Body text**: Clean sans-serif for readability (e.g., Inter, Open Sans)
- **Accent text**: For special elements like campaign names, character names

### UI Components Style
- **Buttons**: Rounded corners (8px), gold gradient for primary actions
- **Cards**: Subtle shadow, slight border with bronze tint, rounded corners (12px)
- **Inputs**: Clean borders, gold focus ring, placeholder text in muted color
- **Icons**: Line icons (Bootstrap Icons or similar), gold/bronze color accents
- **Modals**: Centered, dark overlay, smooth fade-in animation

---

## Pages to Design

### 1. Landing Page (Not Authenticated)

**Hero Section:**
- Large centered logo
- Tagline: "Your platform for epic tabletop adventures"
- Two CTA buttons: "Start Your Adventure" (primary) → Register | "Sign In" (secondary) → Login
- Subtle animated background (stars twinkling, slight mist effect)

**Features Section:**
- 4 feature cards in grid:
  1. **Character Creation** - Icon: person badge - "Create detailed characters for your adventures"
  2. **Campaign Management** - Icon: map/book - "Organize your games and track your progress"
  3. **Integrated Dice Roller** - Icon: dice - "Built-in dice system for all your actions"
  4. **Real-Time Play** - Icon: people/lightning - "Play with friends in real-time"

**Supported Systems Section:**
- 3 cards showing: Generic | D&D 5e | Skyrim (coming soon)

**Footer:**
- Links: About, Contact, Privacy Policy, Terms
- Social links
- Copyright

---

### 2. Login Page

**Layout:** Centered card on subtle background (use logo watermark faded in background)

**Card Content:**
- Logo (smaller version) at top
- Title: "Welcome Back"
- Subtitle: "Sign in to continue your adventure"
- Form fields:
  - Email (with envelope icon)
  - Password (with lock icon + show/hide toggle)
- "Forgot password?" link
- Primary button: "Sign In" (full width)
- Divider: "or"
- Secondary link: "Don't have an account? Register"

**States to design:**
- Default
- Loading (spinner in button)
- Error (red alert banner with message)
- Input validation errors

---

### 3. Register Page

**Layout:** Similar to Login, centered card

**Card Content:**
- Logo at top
- Title: "Begin Your Journey"
- Subtitle: "Create your account to start playing"
- Form fields:
  - Nickname (display name)
  - Email
  - Password (with strength indicator)
  - Confirm Password
- Checkbox: "I accept the Terms of Service and Privacy Policy"
- Primary button: "Create Account"
- Secondary link: "Already have an account? Sign In"

**Password strength indicator:**
- Visual bar (red → orange → green)
- Requirements checklist (8+ chars, uppercase, number, special char)

---

### 4. Home Dashboard (Authenticated)

**Header/Navbar:**
- Logo (left)
- Navigation: Home | My Campaigns | My Characters | Dice Roller
- Right side: Notifications bell (with badge) | User avatar dropdown (Profile, Settings, Logout)
- Theme toggle (sun/moon icon)

**Welcome Section:**
- "Welcome back, [Nickname]!"
- Subtitle: "Ready for your next adventure?"

**Quick Actions Grid (4 cards):**
1. **My Characters** - Icon: person - "Create and manage your characters" → /characters
2. **My Campaigns** - Icon: book - "Explore your ongoing campaigns" → /campaigns
3. **Dice Roller** - Icon: dice - "Roll dice for your actions" → /dice
4. **Settings** - Icon: gear - "Customize your experience" → /settings

**Recent Activity Section:**
- Timeline/list of recent actions
- Empty state: "No recent activity. Start by creating your first character!"

**Active Campaigns Widget:**
- List of campaigns user is part of
- Show: Campaign name, Game type badge, Role (Player/GM), Last activity
- Empty state: "No active campaigns. Join or create one!"

---

### 5. Profile Page

**Layout:** Two-column on desktop, stacked on mobile

**Left Column (Profile Card):**
- Large avatar (with edit overlay on hover)
- Nickname (editable)
- Email (read-only, with verified badge)
- Username (optional, with @ prefix)
- Member since date
- Last login date

**Right Column:**

**Role Management Section:**
- Current roles badges (Player, Game Master)
- "Request Game Master Role" button (if not GM)
- Explanation text for what GM role allows

**Statistics Section:**
- Characters created: X
- Campaigns joined: X
- Sessions played: X
- Dice rolled: X

**Quick Actions:**
- Edit Profile button
- Change Password button
- Link to Settings

---

### 6. Settings Page

**Layout:** Sidebar navigation + content area

**Sidebar Categories:**
- Account
- Appearance
- Notifications
- Privacy
- Danger Zone

**Account Section:**
- Change email
- Change password
- Two-factor authentication (future)

**Appearance Section:**
- Theme toggle: Light / Dark / System
- Language selector: French / English
- Font size preference

**Notifications Section:**
- Email notifications toggles:
  - Session invitations
  - Combat turn reminders
  - Campaign updates
- Push notifications (future)

**Privacy Section:**
- Profile visibility: Public / Friends only / Private
- Show online status toggle

**Danger Zone:**
- Delete account (red button, confirmation modal required)

---

### 7. Campaigns List Page

**Header:**
- Title: "My Campaigns"
- Filters: All | As Player | As Game Master
- Search bar
- "+ Create Campaign" button (if user is GM)

**Campaign Cards Grid:**
Each card shows:
- Campaign banner/image (or default based on game type)
- Campaign name
- Game type badge (Generic/D&D/Skyrim)
- Description (truncated)
- Player count: "3/6 players"
- Your role badge: "Game Master" or "Player"
- Status indicator: Active (green) / Paused (yellow) / Completed (gray)
- Last activity: "2 days ago"
- Click → Campaign detail page

**Empty State:**
- Illustration
- "No campaigns yet"
- "Create your first campaign or ask a Game Master to invite you"
- CTA: "Create Campaign" (if GM) or "Explore Public Campaigns"

---

### 8. Create Campaign Page

**Layout:** Centered form card (max-width 800px)

**Form Sections:**

**Basic Information:**
- Campaign name (required)
- Description (textarea, optional)
- Game type selector: Generic | D&D 5e | Skyrim
- Max players (number input, default 6)

**Visibility:**
- Private (invite only)
- Friends only
- Public (visible in explore)

**Campaign Banner:**
- Image upload zone (drag & drop)
- Or choose from presets

**Preview card** showing how it will look

**Actions:**
- "Create Campaign" primary button
- "Cancel" secondary link

---

### 9. Characters List Page

**Header:**
- Title: "My Characters"
- Filters by game type
- Search bar
- "+ Create Character" button

**Character Cards Grid:**
Each card shows:
- Character portrait (or default avatar)
- Character name
- Class/Role and Level (for D&D)
- Game type badge
- Campaign association (if any)
- Quick stats preview
- Actions: View | Edit | Delete

**Empty State:**
- Illustration of character creation
- "No characters yet"
- "Create your first hero!"
- CTA: "Create Character"

---

### 10. Error Pages

**404 - Not Found:**
- Large "404" with fantasy styling
- Message: "Lost in the mists..."
- Subtitle: "The page you're looking for doesn't exist"
- CTA: "Return to Home"

**500 - Server Error:**
- Message: "The dragon attacked our servers..."
- Subtitle: "Something went wrong. Please try again later."
- CTA: "Return to Home"

**403 - Forbidden:**
- Message: "The gate is sealed..."
- Subtitle: "You don't have permission to access this page"
- CTA: "Return to Home"

---

## Interactive Elements

### Theme Toggle
- Smooth transition (300ms) between themes
- Icon changes: Sun (light mode) ↔ Moon (dark mode)
- Save preference to localStorage
- Respect system preference on first visit

### Loading States
- Skeleton loaders for cards and lists
- Spinner for buttons during submission
- Full-page loader with logo animation for initial app load

### Notifications/Toasts
- Position: Top-right
- Types: Success (green), Error (red), Warning (orange), Info (blue)
- Auto-dismiss after 5 seconds
- Dismiss button (X)

### Modals
- Centered with dark overlay
- Close on overlay click or Escape key
- Smooth scale-in animation
- Confirmation modals for destructive actions (red accent)

### Form Validation
- Real-time validation as user types
- Error messages below inputs (red text)
- Success checkmark when valid
- Focus ring in gold color

---

## Responsive Breakpoints

- **Mobile**: < 640px (single column, hamburger menu)
- **Tablet**: 640px - 1024px (2 columns, collapsible sidebar)
- **Desktop**: > 1024px (full layout, sidebar navigation)

---

## Deliverables Expected

1. **Design System / Style Guide**
   - Color palette (both themes)
   - Typography scale
   - Spacing system
   - Component library (buttons, inputs, cards, etc.)

2. **Page Mockups**
   - All pages listed above
   - Both dark and light theme versions
   - Desktop and mobile versions

3. **Interactive Prototype**
   - Clickable prototype showing navigation flow
   - Theme toggle functionality
   - Form interactions

4. **Assets**
   - Icons (SVG format)
   - Illustrations for empty states and error pages
   - Any decorative elements

---

## Inspiration & References

- **Roll20** - For RPG platform reference
- **D&D Beyond** - For fantasy aesthetic
- **Notion** - For clean, professional UI
- **Discord** - For dark theme reference
- **Linear** - For modern, minimal design approach

---

## Technical Notes

The design will be implemented in **Blazor Server** with **Bootstrap 5** as the base CSS framework. Please ensure:
- CSS variables for theming
- Bootstrap-compatible class naming when possible
- Mobile-first approach
- Smooth animations (prefer CSS transitions over JavaScript)

---

## Condensed Prompt (for AI with character limits)

```
# UI/UX Design - Chronique des Mondes

Tabletop RPG platform (campaigns, characters, combat, dice). Supports D&D 5e, Generic, Skyrim.

**Colors (from logo: medieval book, D20, dragon, castle, gold frame):**
Primary: Gold #C9A227, Bronze #8B6914, Crimson #8B2942
Dark: BG #0D1B2A, Cards #1B2838, Text #E8E6E3
Light: BG #F5F0E6 (parchment), Cards #FDFCFA, Text #2C2416

**Requirements:** Clean, professional, dark/light toggle, fantasy aesthetic, responsive, Serif headings (Cinzel), Sans body (Inter)

**Pages:**
1. Landing: Logo, tagline, CTA (Register/Login), Features (Characters, Campaigns, Dice, Real-time), Systems
2. Login: Centered card, Email/Password, "Forgot password?", Register link
3. Register: Nickname, Email, Password (strength), Confirm, Terms checkbox
4. Dashboard: Navbar (Logo, Nav, Notifications, User, Theme toggle), Welcome, Quick actions grid, Recent activity, Active campaigns
5. Profile: Avatar, Nickname, Email, Roles, Stats (Characters/Campaigns/Sessions/Dice)
6. Settings: Sidebar (Account, Appearance, Notifications, Privacy, Danger), Theme selector
7. Campaigns: Filters, Search, Create button, Cards (Name, Type, Players, Role, Status)
8. Characters: Filters, Search, Create button, Cards (Portrait, Name, Class, Type)
9. Errors: 404/500/403 fantasy-themed

**Components:** Buttons (8px rounded, gold gradient), Cards (12px rounded, bronze border), Inputs (gold focus), Modals (dark overlay), Toasts (top-right)

**Deliverables:** Design system, All pages (dark+light, desktop+mobile), Prototype
Tech: Blazor + Bootstrap 5, CSS variables
```
