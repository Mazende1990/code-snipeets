package java.s5;

import java.util.List;
import java.util.Map;

import javax.validation.Valid;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.samples.petclinic.visit.VisitRepository;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.WebDataBinder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.ModelAndView;

@Controller
class OwnerController {

	private static final String OWNER_FORM_VIEW = "owners/createOrUpdateOwnerForm";
	private static final int PAGE_SIZE = 5;

	private final OwnerRepository ownerRepository;
	private final VisitRepository visitRepository;

	public OwnerController(OwnerRepository ownerRepository, VisitRepository visitRepository) {
		this.ownerRepository = ownerRepository;
		this.visitRepository = visitRepository;
	}

	@InitBinder
	public void setAllowedFields(WebDataBinder dataBinder) {
		dataBinder.setDisallowedFields("id");
	}

	// ------------------- Creation -------------------

	@GetMapping("/owners/new")
	public String initCreationForm(Map<String, Object> model) {
		model.put("owner", new Owner());
		return OWNER_FORM_VIEW;
	}

	@PostMapping("/owners/new")
	public String processCreationForm(@Valid Owner owner, BindingResult result) {
		if (result.hasErrors()) {
			return OWNER_FORM_VIEW;
		}
		ownerRepository.save(owner);
		return "redirect:/owners/" + owner.getId();
	}

	// ------------------- Find/Search -------------------

	@GetMapping("/owners/find")
	public String initFindForm(Map<String, Object> model) {
		model.put("owner", new Owner());
		return "owners/findOwners";
	}

	@GetMapping("/owners")
	public String processFindForm(
			@RequestParam(defaultValue = "1") int page,
			Owner owner,
			BindingResult result,
			Model model) {

		String lastName = (owner.getLastName() == null) ? "" : owner.getLastName();
		Page<Owner> searchResults = findPaginatedOwnersByLastName(page, lastName);

		if (searchResults.isEmpty()) {
			result.rejectValue("lastName", "notFound", "not found");
			return "owners/findOwners";
		} else if (searchResults.getTotalElements() == 1) {
			owner = searchResults.iterator().next();
			return "redirect:/owners/" + owner.getId();
		} else {
			return populateOwnersListModel(model, page, lastName, searchResults);
		}
	}

	private Page<Owner> findPaginatedOwnersByLastName(int page, String lastName) {
		Pageable pageable = PageRequest.of(page - 1, PAGE_SIZE);
		return ownerRepository.findByLastName(lastName, pageable);
	}

	private String populateOwnersListModel(Model model, int page, String lastName, Page<Owner> ownersPage) {
		model.addAttribute("listOwners", ownersPage.getContent());
		model.addAttribute("currentPage", page);
		model.addAttribute("totalPages", ownersPage.getTotalPages());
		model.addAttribute("totalItems", ownersPage.getTotalElements());
		return "owners/ownersList";
	}

	// ------------------- Update -------------------

	@GetMapping("/owners/{ownerId}/edit")
	public String initUpdateForm(@PathVariable("ownerId") int ownerId, Model model) {
		Owner owner = ownerRepository.findById(ownerId);
		model.addAttribute("owner", owner);
		return OWNER_FORM_VIEW;
	}

	@PostMapping("/owners/{ownerId}/edit")
	public String processUpdateForm(
			@Valid Owner owner,
			BindingResult result,
			@PathVariable("ownerId") int ownerId) {

		if (result.hasErrors()) {
			return OWNER_FORM_VIEW;
		}
		owner.setId(ownerId);
		ownerRepository.save(owner);
		return "redirect:/owners/{ownerId}";
	}

	// ------------------- View Owner -------------------

	@GetMapping("/owners/{ownerId}")
	public ModelAndView showOwner(@PathVariable("ownerId") int ownerId) {
		Owner owner = ownerRepository.findById(ownerId);

		// Populate each pet with its visits
		for (Pet pet : owner.getPets()) {
			pet.setVisitsInternal(visitRepository.findByPetId(pet.getId()));
		}

		ModelAndView mav = new ModelAndView("owners/ownerDetails");
		mav.addObject("owner", owner);
		return mav;
	}
}
