import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, Button, Alert, Spinner, Row, Col } from 'react-bootstrap';
import apiClient from '../api/apiClient';
import type { CheckFileInfoResponse } from '../models/types';
import RenameModal from '../components/RenameModal';
import OverwriteModal from '../components/OverwriteModal';
import ConfirmDialog from '../components/ConfirmDialog';
import LockPanel from '../components/LockPanel';

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export default function FileDetailPage() {
  const { fileId } = useParams<{ fileId: string }>();
  const navigate = useNavigate();
  const [info, setInfo] = useState<CheckFileInfoResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [wopiUrl, setWopiUrl] = useState('');
  const [showRename, setShowRename] = useState(false);
  const [showOverwrite, setShowOverwrite] = useState(false);
  const [showDelete, setShowDelete] = useState(false);

  const fetchInfo = useCallback(async () => {
    if (!fileId) return;
    setLoading(true);
    setError('');
    try {
      const { data } = await apiClient.get<CheckFileInfoResponse>(
        `/storage/${encodeURIComponent(fileId)}`,
      );
      setInfo(data);
    } catch {
      setError('Failed to load file info.');
    } finally {
      setLoading(false);
    }
  }, [fileId]);

  useEffect(() => {
    fetchInfo();
  }, [fetchInfo]);

  const handleDownload = async () => {
    if (!fileId) return;
    try {
      const { data } = await apiClient.get(
        `/storage/${encodeURIComponent(fileId)}/download`,
        { responseType: 'blob' },
      );
      const url = URL.createObjectURL(data);
      const a = document.createElement('a');
      a.href = url;
      a.download = info?.baseFileName || 'download';
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      setError('Download failed.');
    }
  };

  const handleDelete = async () => {
    if (!fileId) return;
    try {
      await apiClient.delete(`/storage/${encodeURIComponent(fileId)}`);
      navigate('/files');
    } catch {
      setError('Delete failed.');
      setShowDelete(false);
    }
  };

  const handleWopiUrl = async () => {
    if (!fileId) return;
    setError('');
    try {
      const { data } = await apiClient.get<string>(
        `/wopi/${encodeURIComponent(fileId)}/urlBuilder`,
      );
      setWopiUrl(data);
    } catch {
      setError('Failed to get WOPI URL.');
    }
  };

  if (loading) {
    return (
      <div className="text-center mt-5">
        <Spinner animation="border" />
      </div>
    );
  }

  if (!info) {
    return <Alert variant="danger">{error || 'File not found.'}</Alert>;
  }

  return (
    <>
      <Button variant="outline-secondary" size="sm" className="mb-3" onClick={() => navigate('/files')}>
        &larr; Back to Files
      </Button>

      {error && <Alert variant="danger" dismissible onClose={() => setError('')}>{error}</Alert>}

      <Card>
        <Card.Header>
          <h4 className="mb-0">{info.baseFileName}</h4>
        </Card.Header>
        <Card.Body>
          <Row>
            <Col sm={6}>
              <p><strong>File ID:</strong> {fileId}</p>
              <p><strong>Size:</strong> {formatSize(info.size)}</p>
              <p><strong>Version:</strong> {info.version}</p>
            </Col>
            <Col sm={6}>
              <p><strong>Owner ID:</strong> {info.ownerId}</p>
              <p><strong>User ID:</strong> {info.userId}</p>
              <p><strong>Writable:</strong> {info.userCanWrite ? 'Yes' : 'No'}</p>
            </Col>
          </Row>

          <div className="d-flex gap-2 mt-3 flex-wrap">
            <Button variant="success" size="sm" onClick={handleDownload}>
              Download
            </Button>
            <Button variant="primary" size="sm" onClick={() => setShowRename(true)}>
              Rename
            </Button>
            <Button variant="warning" size="sm" onClick={() => setShowOverwrite(true)}>
              Overwrite
            </Button>
            <Button variant="danger" size="sm" onClick={() => setShowDelete(true)}>
              Delete
            </Button>
            <Button variant="info" size="sm" onClick={handleWopiUrl}>
              Get WOPI URL
            </Button>
          </div>

          {wopiUrl && (
            <Alert variant="info" className="mt-3" dismissible onClose={() => setWopiUrl('')}>
              <strong>WOPI URL:</strong>{' '}
              <a href={wopiUrl} target="_blank" rel="noopener noreferrer">
                {wopiUrl}
              </a>
            </Alert>
          )}
        </Card.Body>
      </Card>

      <LockPanel fileId={fileId!} />

      <RenameModal
        show={showRename}
        fileId={fileId!}
        currentName={info.baseFileName}
        onClose={() => setShowRename(false)}
        onRenamed={fetchInfo}
      />

      <OverwriteModal
        show={showOverwrite}
        fileId={fileId!}
        onClose={() => setShowOverwrite(false)}
        onOverwritten={fetchInfo}
      />

      <ConfirmDialog
        show={showDelete}
        title="Delete File"
        message={`Are you sure you want to delete "${info.baseFileName}"?`}
        onConfirm={handleDelete}
        onCancel={() => setShowDelete(false)}
      />
    </>
  );
}
