import { useState, useEffect } from 'react';
import { Modal, Button, Form, Alert } from 'react-bootstrap';
import apiClient from '../api/apiClient';

interface Props {
  show: boolean;
  fileId: string;
  currentName: string;
  onClose: () => void;
  onRenamed: () => void;
}

export default function RenameModal({ show, fileId, currentName, onClose, onRenamed }: Props) {
  const [newName, setNewName] = useState(currentName);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (show) setNewName(currentName);
  }, [show, currentName]);

  const handleRename = async () => {
    if (!newName.trim()) {
      setError('Name cannot be empty.');
      return;
    }
    setError('');
    setLoading(true);
    try {
      await apiClient.post(`/storage/${encodeURIComponent(fileId)}/rename`, {
        baseFileName: newName,
      });
      onRenamed();
      onClose();
    } catch {
      setError('Rename failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal show={show} onHide={onClose} centered>
      <Modal.Header closeButton>
        <Modal.Title>Rename File</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {error && <Alert variant="danger">{error}</Alert>}
        <Form.Group>
          <Form.Label>New file name</Form.Label>
          <Form.Control
            type="text"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
          />
        </Form.Group>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button variant="primary" onClick={handleRename} disabled={loading}>
          {loading ? 'Renaming...' : 'Rename'}
        </Button>
      </Modal.Footer>
    </Modal>
  );
}
