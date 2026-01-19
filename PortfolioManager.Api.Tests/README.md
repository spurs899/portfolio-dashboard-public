# Setup Instructions

## Configuration

Before running tests, you need to create your own `appsettings.json` file with your credentials.

1. Copy `appsettings.template.json` to `appsettings.json`:
   ```powershell
   Copy-Item appsettings.template.json appsettings.json
   ```

2. Edit `appsettings.json` and add your credentials:
   ```json
   {
     "Sharesies": {
       "Email": "your-actual-email@example.com",
       "Password": "your-actual-password"
     },
     "IBKR": {
       "Username": "your-actual-username",
       "Password": "your-actual-password"
     }
   }
   ```

**Note**: The `appsettings.json` file is git-ignored to prevent accidentally committing credentials.

## Running Integration Tests

### Sharesies Tests
The Sharesies integration tests (`SharesiesClientIntegrationTests.cs`) are skipped by default. To run them:

1. Add your Sharesies credentials to `appsettings.json`
2. Remove the `Skip` attribute from the test you want to run
3. For MFA-protected accounts, you'll need to run the test once to get the MFA prompt, then update the `mfaCode` variable and re-run

### IBKR Tests
The IBKR integration tests (`IbkrClientIntegrationTests.cs`) require QR code authentication:

1. Add your IBKR credentials to `appsettings.json`
2. Remove the `Skip` attribute from the test you want to run
3. **Important**: Have your phone ready with the IBKR Mobile app
4. When you run the `FullIntegrationFlow_ShouldSucceed` test:
   - The test will initiate authentication
   - You'll have 30 seconds to scan the QR code on your mobile device
   - The test will poll for authentication completion
   - Once authenticated, it will test all API endpoints

**Available IBKR Tests:**
- `InitializeAuthentication_ShouldReturnAuthResponse` - Tests initial auth request
- `FullIntegrationFlow_ShouldSucceed` - Full flow with QR authentication and portfolio retrieval
- `ValidateSso_WithoutAuthentication_ShouldFail` - Tests unauthenticated SSO validation
- `GetAccounts_WithoutAuthentication_ShouldReturnNull` - Tests unauthenticated account access
- `GetPositions_WithValidAccountId_ShouldReturnPositions` - Tests position retrieval with active session
