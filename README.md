# Health System

A Unity package providing an out-of-the-box health system with event-driven architecture and built-in UI components.

## Features

- **Simple Health Component** - Easy-to-use health management with current and max health values
- **Automatic Clamping** - Health values are automatically clamped between 0 and max
- **Event System** - Comprehensive UnityEvents for health changes (damage, healing, zero, max, etc.)
- **UI Components** - Ready-to-use UI components for displaying health (slider and text)
- **Custom Inspector** - Visual health bar and quick test buttons in the Unity Inspector
- **Editor Tools** - Context menu actions and inspector buttons for easy testing

## Installation

### Via Package Manager

1. Open Unity Package Manager (Window → Package Manager)
2. Click the **+** button and select **Add package from git URL**
3. Enter the package URL:
   ```
   https://github.com/jacobhomanics/health-system.git
   ```
   Or use the local path if installed locally.

### Manual Installation

1. Clone or download this repository
2. Copy the `health-system` folder into your Unity project's `Packages` folder
3. The package will be automatically recognized by Unity

## Quick Start

### Basic Setup

1. Add the `Health` component to any GameObject:

   - Select your GameObject in the hierarchy
   - Click **Add Component** → Search for **Health**
   - Set the **Max Health** value in the inspector

2. The health component automatically provides:
   - Current health (clamped between 0 and max)
   - Max health value
   - Visual health bar in the inspector
   - Quick test buttons for damage/healing

### Using Health in Code

```csharp
using JacobHomanics.HealthSystem;

// Get the Health component
Health health = GetComponent<Health>();

// Deal damage
health.Current -= 10;

// Heal
health.Current += 5;

// Set health directly
health.Current = 50;

// Access max health
float maxHP = health.Max;
health.Max = 100; // Increase max health
```

## Components

### Health

The core component that manages health values and events.

**Properties:**

- `Current` (float) - Current health value (automatically clamped between 0 and max)
- `Max` (float) - Maximum health value

**Events:**

- `onCurrentSet` - Invoked whenever Current is set
- `onCurrentChange` - Invoked when Current changes (up or down)
- `onCurrentDown` - Invoked when Current decreases
- `onCurrentUp` - Invoked when Current increases
- `onCurrentZero` - Invoked when Current reaches 0
- `onCurrentMax` - Invoked when Current reaches Max
- `onMaxSet` - Invoked whenever Max is set
- `onMaxChange` - Invoked when Max changes
- `onMaxDown` - Invoked when Max decreases
- `onMaxUp` - Invoked when Max increases

**Example Usage:**

```csharp
using JacobHomanics.HealthSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Health health;

    void Start()
    {
        health = GetComponent<Health>();

        // Subscribe to health events
        health.onCurrentZero.AddListener(OnDeath);
        health.onCurrentDown.AddListener(OnTakeDamage);
    }

    void OnDeath()
    {
        Debug.Log("Player died!");
        // Handle death logic
    }

    void OnTakeDamage()
    {
        Debug.Log($"Health: {health.Current}/{health.Max}");
        // Play damage effects, etc.
    }
}
```

### HealthSlider

A UI component that automatically syncs a Unity UI Slider with a Health component.

**Setup:**

1. Create a UI Slider (GameObject → UI → Slider)
2. Add the `HealthSlider` component to the Slider GameObject
3. Assign the Health component reference in the inspector
4. The slider will automatically update to reflect the health value

**Properties:**

- `health` (Health) - Reference to the Health component
- `slider` (Slider) - Reference to the UI Slider component

### HealthText

A UI component that displays health values using TextMeshPro.

**Setup:**

1. Create a TextMeshPro Text component (GameObject → UI → Text - TextMeshPro)
2. Add the `HealthText` component to the same GameObject
3. Assign the Health component reference
4. Select the display type (Current, Max, or Difference)

**Properties:**

- `health` (Health) - Reference to the Health component
- `text` (TMP_Text) - Reference to the TextMeshPro Text component
- `displayType` (enum) - Display mode:
  - **Current** - Shows current health value
  - **Max** - Shows max health value
  - **Difference** - Shows damage taken (max - current)

## Inspector Features

The custom Health Inspector provides:

- **Visual Health Bar** - Color-coded health bar (green → yellow → red)
- **Quick Test Buttons**:
  - Damage 1 / Damage 10
  - Heal 1 / Heal 10
  - Set to 0 / Set to Max
- **Event Management** - Organized tabs for Current Health and Max Health events
- **Real-time Updates** - Health bar and values update in real-time

## Context Menu Actions

Right-click the Health component in the inspector for quick access to:

- Damage 1
- Damage 10
- Heal 1
- Heal 10

## Examples

### Damage System

```csharp
public class DamageDealer : MonoBehaviour
{
    public float damageAmount = 10f;

    void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.Current -= damageAmount;
        }
    }
}
```

### Health Pickup

```csharp
public class HealthPickup : MonoBehaviour
{
    public float healAmount = 20f;

    void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null && health.Current < health.Max)
        {
            health.Current += healAmount;
            Destroy(gameObject);
        }
    }
}
```

### Health Regeneration

```csharp
public class HealthRegeneration : MonoBehaviour
{
    public float regenRate = 1f; // Health per second
    private Health health;

    void Start()
    {
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (health.Current < health.Max)
        {
            health.Current += regenRate * Time.deltaTime;
        }
    }
}
```

## Requirements

- Unity 6000.0.31f1 or later
- TextMeshPro (for HealthText component)

## License

See LICENSE.md file for details.

## Author

**Jacob Homanics**

- Email: homanicsjake@gmail.com
- Website: jacobhomanics.com

## Support

For issues, questions, or contributions, please visit the project repository.
