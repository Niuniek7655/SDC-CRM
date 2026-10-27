// Local protocol double for the SMOKE RUNNER, not the CRM or JWT implementation.
import http from 'node:http';
import { randomBytes } from 'node:crypto';
export function fixture(mode) {
  const rows = [];
  let sequence = 0;
  return http.createServer(async (req, res) => {
    const sent = req.headers['x-correlation-id'];
    res.setHeader('X-Correlation-ID', typeof sent === 'string' && /^[A-Za-z0-9_.:-]{1,64}$/.test(sent) ? sent : randomBytes(16).toString('hex'));
    res.setHeader('Content-Type', 'application/json');
    const send = (code, value) => { res.statusCode = code; res.end(JSON.stringify(value)); };
    if (req.url === '/openapi/v1.json') return send(200, { paths: { '/api/leads': {}, '/api/leads/mine': {} } });
    let subject;
    try { subject = JSON.parse(Buffer.from(req.headers.authorization.split(' ')[1].split('.')[1], 'base64url').toString()).sub; } catch {}
    if (!['sales-a', 'sales-b', 'backoffice'].includes(subject)) return send(401, {});
    if (subject === 'backoffice') return send(403, {});
    if (req.method === 'GET' && req.url === '/api/leads/mine') {
      if (mode === 'empty-b' && subject === 'sales-b') return send(200, []);
      const visible = rows.filter(x => mode === 'leak' || x.owner === subject).sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
      return send(200, visible.map(({ owner, ...row }) => mode === 'corrupt-data' ? { ...row, contactName: 'Wrong value' } : row));
    }
    if (req.method === 'POST' && req.url === '/api/leads') {
      let body;
      try { const chunks = []; for await (const chunk of req) chunks.push(chunk); body = JSON.parse(Buffer.concat(chunks).toString()); } catch { return send(400, {}); }
      if (!body.companyName?.trim()) return send(400, { detail: 'A lead must have a company name.' });
      if (!body.contactName?.trim()) return send(400, { detail: 'A lead must have a contact name.' });
      if (!body.contactEmail || !body.contactEmail.includes('@')) return send(400, { detail: 'Contact e-mail is not valid.' });
      const id = '00000000-0000-4000-8000-' + (++sequence).toString().padStart(12, '0');
      rows.push({ ...body, id, owner: subject, status: 'New', createdAtUtc: new Date(Date.UTC(2026, 0, 1, 0, 0, sequence)).toISOString() });
      res.setHeader('Location', '/api/leads/mine');
      return send(201, { id });
    }
    send(404, {});
  });
}
