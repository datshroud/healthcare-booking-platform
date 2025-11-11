document.addEventListener('DOMContentLoaded', () => {
	const uploadInput = document.getElementById('avatarUpload');
	const previewImg = document.getElementById('avatarPreview');
	const triggerBtn = document.getElementById('avatarUploadBtn');

	if (!uploadInput || !previewImg || !triggerBtn) {
		return;
	}

	const openPicker = () => uploadInput.click();
	previewImg.addEventListener('click', openPicker);
	triggerBtn.addEventListener('click', openPicker);

	uploadInput.addEventListener('change', () => {
		const file = uploadInput.files?.[0];
		if (!file) {
			return;
		}

		const reader = new FileReader();
		reader.onload = event => {
			if (typeof event.target?.result === 'string') {
				previewImg.src = event.target.result;
			}
		};
		reader.readAsDataURL(file);
	});
});
