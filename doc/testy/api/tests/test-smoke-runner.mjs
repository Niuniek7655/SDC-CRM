// Fault injection: validate that the smoke runner rejects incomplete/broken responses.
import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { fixture } from './smoke-fixture.mjs';
import { readFileSync } from 'node:fs';
import { Script } from 'node:vm';
const smoke = fileURLToPath(new URL('../test-api.ps1', import.meta.url));
const token = sub => 'test.' + Buffer.from(JSON.stringify({ sub })).toString('base64url') + '.signature';

const powershell = readFileSync(smoke, 'utf8');
const httpFile = readFileSync(new URL('../crm-api.http', import.meta.url), 'utf8');
const psIds = new Set([...powershell.matchAll(/["'](API-\d+)["']/g)].map(x => x[1]));
const httpIds = new Set([...httpFile.matchAll(/client\.test\("(API-\d+):/g)].map(x => x[1]));
for (const id of psIds) if (!httpIds.has(id)) throw new Error('Missing HTTP Client assertion: ' + id);
for (const match of httpFile.matchAll(/>\s*\{%([\s\S]*?)%\}/g)) new Script(match[1]);
console.log('PASS HTTP Client parity and handler syntax:', psIds.size, 'smoke cases');
let failures = 0;
for (const [mode, kind, expected] of [
  ['good', 'full', 0], ['good', 'partial', 2], ['good', 'required', 2],
  ['good', 'same-subject', 1], ['empty-b', 'full', 1], ['leak', 'full', 1], ['corrupt-data', 'full', 1],
]) {
  const server = fixture(mode);
  await new Promise((resolve, reject) => { server.once('error', reject); server.listen(0, '127.0.0.1', resolve); });
  try {
    const args = ['-NoProfile', '-File', smoke, '-BaseUrl', 'http://127.0.0.1:' + server.address().port];
    if (kind === 'full' || kind === 'same-subject') args.push('-SalespersonToken', token('sales-a'), '-OtherSalesToken', token(kind === 'same-subject' ? 'sales-a' : 'sales-b'), '-BackofficeToken', token('backoffice'), '-RequireAllTokens');
    if (kind === 'required') args.push('-RequireAllTokens');
    const { code, output } = await new Promise((resolve, reject) => {
      const child = spawn(process.env.SDC_CRM_POWERSHELL || 'pwsh', args, { windowsHide: true, timeout: 30000 });
      let output = '';
      child.stdout.on('data', x => output += x);
      child.stderr.on('data', x => output += x);
      child.once('error', reject);
      child.once('close', code => resolve({ code, output }));
    });
    if (code !== expected) { failures++; console.error('FAIL', mode, kind, 'exit', code, 'expected', expected, '\n' + output); }
    else console.log('PASS', mode, kind, 'exit', code);
  } finally { await new Promise(resolve => server.close(resolve)); }
}
process.exitCode = failures ? 1 : 0;
