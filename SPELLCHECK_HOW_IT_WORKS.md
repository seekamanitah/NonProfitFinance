# Spell Check - How It Works & Expected Behavior

## ?? Important: This is NOT Auto-Correct

The spell check feature in this application uses **browser native spell checking**, which means:

? **What it DOES:**
- Underlines misspelled words with a red squiggly line
- Provides suggestions when you right-click on misspelled words
- Works in all text inputs and textareas
- Supports custom nonprofit/finance terms

? **What it DOES NOT DO:**
- Automatically correct your typing
- Change words as you type
- Block you from entering misspelled words
- Show popup corrections

This is by design - professional financial software should NOT auto-correct, as it could:
- Change important financial terms incorrectly
- Modify account names or payee names
- Alter transaction descriptions
- Create compliance issues

## How to Use Spell Check

### 1. Enable Spell Check
- Go to **Settings ? Accessibility**
- Toggle "Enable Spell Check" ON

### 2. Using Spell Check
1. Type in any text field (description, notes, etc.)
2. Misspelled words will be underlined in red
3. Right-click on the red underlined word
4. Select the correct spelling from the suggestions
5. Or click "Add to Dictionary" to add custom terms

### 3. Where Spell Check Works
? **Enabled on:**
- Transaction descriptions
- Transaction payee fields
- Notes and comments
- Document descriptions
- Grant/Donor notes
- Report descriptions
- Any textarea or text input

? **Disabled on:**
- Password fields
- Email fields
- Number fields
- Phone fields
- URL fields

## Custom Dictionary

The system includes common nonprofit and finance terms:

**Nonprofit Terms:**
- nonprofit, nonprofits
- 501c3
- fundraiser, fundraising
- grantor, grantee, donee
- unrestricted, endowment
- bylaws

**Financial Terms:**
- reconciliation
- reimbursement
- budgeted, unbudgeted
- subcategory, subcategories
- payee, payer

**Form Terms:**
- reimbursable
- checkbox, dropdown, textarea

**Organization-Specific:**
- AFG, FEMA, SAFER, SCBA, PPE
- (These can be customized)

## Testing Spell Check

### Test These Words:
1. Type: "receit" ? Should show red underline, suggest "receipt"
2. Type: "incoem" ? Should show red underline, suggest "income"  
3. Type: "nonprofit" ? Should NOT show red underline (in dictionary)
4. Type: "501c3" ? Should NOT show red underline (in dictionary)
5. Type: "asdf" ? Should show red underline (not a word)

### If Spell Check Isn't Working:

**Check 1: Is it enabled?**
- Settings ? Accessibility ? Spell Check toggle should be ON

**Check 2: Browser Support**
- Chrome/Edge: ? Full support
- Firefox: ? Full support  
- Safari: ? Full support
- IE11: ? Limited support

**Check 3: Browser Spell Check Settings**
- **Chrome:** Settings ? Languages ? Spell check (should be ON)
- **Firefox:** about:config ? layout.spellcheckDefault (should be 2)
- **Edge:** Settings ? Languages ? Spell check (should be ON)

**Check 4: Field Type**
- Make sure you're typing in a text field, not a number or email field
- Some fields intentionally have spell check disabled

## Why Not Auto-Correct?

### Reasons for Manual Correction Only:

1. **Accuracy**: Financial data must be precise
   - "John's Bakery" should not become "Johns Bakery"
   - "AFG Grant" should not become "AFG Granted"

2. **Legal/Compliance**: Original text must be preserved
   - Grant names must match exactly
   - Donor names cannot be altered
   - Transaction descriptions need to be verbatim

3. **Professional Standards**: Business software standard
   - Microsoft Word uses underlines, not auto-correct by default
   - Excel does not auto-correct cell values
   - Accounting software never auto-corrects

4. **User Control**: You decide what to change
   - Review suggestions before accepting
   - Choose between multiple suggestions
   - Add organization-specific terms to dictionary

## Adding Custom Terms

To add organization-specific terms to the dictionary:

### Method 1: Right-Click Menu
1. Type a word that shows as misspelled
2. Right-click on the red-underlined word
3. Click "Add to Dictionary"
4. Word will no longer show as misspelled

### Method 2: Settings (Future Enhancement)
- Go to Settings ? Spell Check
- Add custom terms to organization dictionary
- Terms will be recognized across all users

## Browser Spell Check vs Application Spell Check

### Browser Native (Current Implementation):
- Uses system/browser dictionary
- Fast and reliable
- No server processing needed
- Supports all languages installed on system
- Free (no API costs)

### Custom Spell Check (Not Implemented):
- Would require external API or library
- Monthly costs for API usage
- Slower performance
- May have usage limits
- Privacy concerns (sending data to external service)

## Troubleshooting

### Problem: No red underlines appear

**Solution 1:** Check browser spell check is enabled
- Chrome: chrome://settings/languages
- Firefox: Preferences ? Language ? Check spelling
- Edge: edge://settings/languages

**Solution 2:** Ensure English dictionary is installed
- Windows: Settings ? Time & Language ? Language
- Mac: System Preferences ? Keyboard ? Text
- Linux: Install aspell-en or hunspell-en

**Solution 3:** Restart browser
- Close all browser windows
- Reopen and try again

### Problem: Wrong language suggestions

**Solution:** Change browser language to English (US)
- Chrome/Edge: Settings ? Languages ? Move English to top
- Firefox: Preferences ? Language ? Set English as primary

### Problem: Custom terms still show as misspelled

**Solution:** Clear and rebuild dictionary
1. Right-click in any text field
2. Select "Spell check" or "Language"
3. Choose "Add to Dictionary" for each term
4. Or add terms via Settings ? Spell Check

## Technical Implementation

### How It Works:
```javascript
// Enable spell check on text inputs
element.setAttribute('spellcheck', 'true');
element.setAttribute('lang', 'en-US');

// Initialize with custom dictionary
spellCheck.initialize(true, customDictionary);
```

### Custom Dictionary Loading:
```csharp
// Server-side custom terms
private static readonly string[] CustomDictionary = new[]
{
    "nonprofit", "501c3", "fundraising",
    "reimbursement", "reconciliation",
    // ... more terms
};
```

### Dynamic Application:
- Mutation observer watches for new input fields
- Automatically applies spell check to dynamically added forms
- Maintains settings across page navigation

## Comparison with Other Software

### Similar Behavior:
- **QuickBooks**: Uses browser native spell check
- **Xero**: Uses browser native spell check
- **Microsoft Dynamics**: Uses browser native spell check
- **Salesforce**: Uses browser native spell check

### Our Approach:
? Industry standard behavior
? No unexpected auto-corrections
? User maintains full control
? Privacy-friendly (no external APIs)
? Free and unlimited usage

## Conclusion

The spell check feature is **working as designed**. It provides:
- Visual indicators of potential typos (red underlines)
- Suggestions via right-click context menu
- Custom nonprofit/finance dictionary
- Professional, non-intrusive behavior

It does **NOT** auto-correct because that would be inappropriate for financial software where accuracy and legal compliance are critical.

If you need auto-correct behavior, that would require a significant architectural change and is not recommended for professional financial applications.
