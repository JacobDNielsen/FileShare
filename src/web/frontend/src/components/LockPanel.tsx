import { useState, useEffect, useRef } from 'react';
import { Card, Button, Alert, Form } from 'react-bootstrap';
import apiClient from '../api/apiClient';

interface Props {
  fileId: string;
}

async function getLockStatus(fileId: string) {
  const res = await apiClient.get(`/lock/${encodeURIComponent(fileId)}`);
  return (res.headers['x-wopi-lock'] as string) || '';
}

export default function LockPanel({ fileId }: Props) {
  const [lockValue, setLockValue] = useState<string>('');
  const [inputLockId, setInputLockId] = useState('');
  const [status, setStatus] = useState('');
  const [error, setError] = useState('');
  const [refreshKey, setRefreshKey] = useState(0);
  const isMounted = useRef(true);

  useEffect(() => {
    isMounted.current = true;
    let cancelled = false;
    getLockStatus(fileId).then((lock) => {
      if (cancelled) return;
      setLockValue(lock);
      setStatus(lock ? `Locked: ${lock}` : 'Unlocked');
    }).catch(() => {
      if (!cancelled) setError('Failed to get lock status.');
    });
    return () => { cancelled = true; };
  }, [fileId, refreshKey]);

  const refresh = () => setRefreshKey((k) => k + 1);

  const acquireLock = async () => {
    const id = inputLockId.trim() || crypto.randomUUID();
    setError('');
    try {
      await apiClient.post(`/lock/${encodeURIComponent(fileId)}`, null, {
        headers: { 'X-WOPI-Lock': id },
      });
      setInputLockId(id);
      refresh();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { status?: number; headers?: Record<string, string> } };
        if (axiosErr.response?.status === 409) {
          const existing = axiosErr.response.headers?.['x-wopi-lock'] || 'unknown';
          setError(`Lock conflict. Existing lock: ${existing}`);
          refresh();
          return;
        }
      }
      setError('Failed to acquire lock.');
    }
  };

  const refreshLock = async () => {
    const id = inputLockId.trim() || lockValue;
    if (!id) {
      setError('No lock ID to refresh.');
      return;
    }
    setError('');
    try {
      await apiClient.post(`/lock/${encodeURIComponent(fileId)}/refresh`, null, {
        headers: { 'X-WOPI-Lock': id },
      });
      refresh();
    } catch {
      setError('Failed to refresh lock.');
    }
  };

  const unlock = async () => {
    const id = inputLockId.trim() || lockValue;
    if (!id) {
      setError('No lock ID to unlock.');
      return;
    }
    setError('');
    try {
      await apiClient.delete(`/lock/${encodeURIComponent(fileId)}`, {
        headers: { 'X-WOPI-Lock': id },
      });
      setInputLockId('');
      refresh();
    } catch {
      setError('Failed to unlock.');
    }
  };

  return (
    <Card className="mt-3">
      <Card.Header>Lock Management</Card.Header>
      <Card.Body>
        {error && <Alert variant="danger">{error}</Alert>}
        <p>
          <strong>Status:</strong> {status || 'Loading...'}
        </p>
        <Form.Group className="mb-3">
          <Form.Label>Lock ID (auto-generated if empty)</Form.Label>
          <Form.Control
            type="text"
            value={inputLockId}
            onChange={(e) => setInputLockId(e.target.value)}
            placeholder="Leave empty to auto-generate"
          />
        </Form.Group>
        <div className="d-flex gap-2">
          <Button variant="primary" size="sm" onClick={acquireLock}>
            Acquire Lock
          </Button>
          <Button variant="secondary" size="sm" onClick={refreshLock}>
            Refresh Lock
          </Button>
          <Button variant="warning" size="sm" onClick={unlock}>
            Unlock
          </Button>
          <Button variant="outline-secondary" size="sm" onClick={refresh}>
            Refresh Status
          </Button>
        </div>
      </Card.Body>
    </Card>
  );
}
