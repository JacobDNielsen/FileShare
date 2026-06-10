import { useState, useRef } from 'react';
import { Modal, Button, Form, Spinner, Alert } from 'react-bootstrap';
import apiClient from '../api/apiClient';

interface Props {
  show: boolean;
  fileId: string;
  onClose: () => void;
  onOverwritten: () => void;
}

export default function OverwriteModal({ show, fileId, onClose, onOverwritten }: Props) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleOverwrite = async () => {
    const file = fileInputRef.current?.files?.[0];
    if (!file) {
      setError('Please select a file.');
      return;
    }
    setError('');
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('File', file);
      await apiClient.post(`/storage/${encodeURIComponent(fileId)}/overwrite`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      onOverwritten();
      handleClose();
    } catch {
      setError('Overwrite failed.');
    } finally {
      setUploading(false);
    }
  };

  const handleClose = () => {
    setError('');
    if (fileInputRef.current) fileInputRef.current.value = '';
    onClose();
  };

  return (
    <Modal show={show} onHide={handleClose} centered>
      <Modal.Header closeButton>
        <Modal.Title>Overwrite File Content</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {error && <Alert variant="danger">{error}</Alert>}
        <Form.Group>
          <Form.Label>Choose replacement file</Form.Label>
          <Form.Control type="file" ref={fileInputRef} />
        </Form.Group>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={handleClose} disabled={uploading}>
          Cancel
        </Button>
        <Button variant="warning" onClick={handleOverwrite} disabled={uploading}>
          {uploading ? <Spinner size="sm" animation="border" /> : 'Overwrite'}
        </Button>
      </Modal.Footer>
    </Modal>
  );
}
