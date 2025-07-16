# 🚀 QUICK SETUP SCRIPT - Hover Highlight Implementation

## Copy-Paste Unity Setup Instructions

### Step 1: Create Hover Highlight Material
```csharp
// In Unity, go to Assets > Create > Material
// Name it "HoverHighlightMaterial"
// Set these properties:
// - Rendering Mode: Transparent
// - Albedo: Yellow (R:1, G:1, B:0, A:0.3)
// - Smoothness: 0.5
// - Metallic: 0
```

### Step 2: Create Hover Highlight GameObject
```csharp
// In Unity Hierarchy:
// 1. Right-click > 3D Object > Quad
// 2. Rename to "HoverHighlight" 
// 3. Set Transform:
//    - Position: (0, 0.01, 0)
//    - Rotation: (90, 0, 0) 
//    - Scale: (0.9, 0.9, 1)
// 4. Assign HoverHighlightMaterial to MeshRenderer
// 5. Initially set GameObject to INACTIVE
```

### Step 3: Assign to GameManager
```csharp
// 1. Select GameManager in scene
// 2. In Inspector, find "Hover Highlight" field
// 3. Drag HoverHighlight GameObject from Hierarchy
// 4. Configure colors:
//    - Hover Color: (1, 1, 0, 0.3) - Yellow
//    - Valid Move Color: (0, 1, 0, 0.5) - Green  
//    - Invalid Move Color: (1, 0, 0, 0.3) - Red
//    - Height Offset: 0.005
```

### Step 4: Test the System
```csharp
// 1. Play the scene
// 2. Move mouse over tiles - should see yellow highlight
// 3. Click piece, hover over valid moves - should see green
// 4. Hover over invalid moves - should see red
```

## Alternative: Programmatic Setup (Advanced)

If you want to create the hover highlight programmatically, add this to GameManager Start():

```csharp
private void CreateHoverHighlightIfMissing()
{
    if (hoverHighlight == null)
    {
        // Create hover highlight GameObject
        hoverHighlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hoverHighlight.name = "HoverHighlight";
        hoverHighlight.transform.rotation = Quaternion.Euler(90, 0, 0);
        hoverHighlight.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        
        // Create and assign material
        Material hoverMaterial = new Material(Shader.Find("Standard"));
        hoverMaterial.SetFloat("_Mode", 3); // Transparent mode
        hoverMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        hoverMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        hoverMaterial.SetInt("_ZWrite", 0);
        hoverMaterial.DisableKeyword("_ALPHATEST_ON");
        hoverMaterial.EnableKeyword("_ALPHABLEND_ON");
        hoverMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        hoverMaterial.renderQueue = 3000;
        hoverMaterial.color = hoverColor;
        
        hoverHighlight.GetComponent<Renderer>().material = hoverMaterial;
        hoverHighlight.SetActive(false);
        
        Debug.Log("✅ Hover highlight created programmatically!");
    }
}
```

## Testing Checklist ✅

- [ ] Hover highlight appears when mouse moves over tiles
- [ ] Highlight disappears when mouse leaves board
- [ ] Green highlight shows for valid moves when piece selected
- [ ] Red highlight shows for invalid moves when piece selected  
- [ ] Yellow highlight shows when no piece selected
- [ ] Highlight clears when game ends (checkmate/stalemate)
- [ ] Highlight works with all piece types
- [ ] No performance issues during hover

**Ready to enhance your chess experience!** 🎯
